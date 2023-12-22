using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data.Entities;
using BlazorAdmin.Im.Events;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlazorAdmin.Im.Components
{
    public partial class ChatDialog
    {
        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        private string _textValue = string.Empty;

        private string _messageValue = string.Empty;

        private object? _selectedValue;

        private MudListItem? _selectedItem;

        private List<ChannelModel> Channels = new List<ChannelModel>();

        private List<MessageModel> MessageModels = new List<MessageModel>();

        private List<UserModel> UserModels = new List<UserModel>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var scrollModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorAdmin.Im/js/scroll.js");
            await scrollModule.InvokeVoidAsync("scrollToBottom");
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitialChannelList();

            if (Channels.Count > 0)
            {
                _selectedValue = Channels.First();
            }
            await InitialMessage();
        }

        private async Task SelectedChannelChanged(object selectedValue)
        {
            _selectedValue = selectedValue;
            await InitialMessage();
        }

        private async Task InitialChannelList()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();

            using var context = await _dbFactory.CreateDbContextAsync();
            var channels = (from c in context.ChatChannels
                            join cm in context.ChatChannelMembers on c.Id equals cm.ChatChannelId
                            where cm.MemberId == state.User.GetUserId()
                            select c).AsNoTracking().ToList();

            var channelIds = channels.Where(c => c.Type != (int)ChatChannelType.普通群聊).Select(c => c.Id);
            var userList = (from cm in context.ChatChannelMembers
                            join u in context.Users on cm.MemberId equals u.Id
                            where channelIds.Contains(cm.MemberId) && cm.MemberId != state.User.GetUserId()
                            select new
                            {
                                cm.ChatChannelId,
                                u.RealName,
                                u.Avatar,
                            }).AsNoTracking().ToList();

            var noReads = context.ChatMessageNoReads.Where(r => r.ReciverId == state.User.GetUserId()).ToList();

            foreach (var channel in channels)
            {
                var channelName = channel.Name;
                var avatar = string.Empty;
                if (channel.Type != (int)ChatChannelType.普通群聊)
                {
                    var findUser = userList.FirstOrDefault(u => u.ChatChannelId == channel.Id);
                    channelName = findUser?.RealName;
                    avatar = findUser?.Avatar;
                }
                Channels.Add(new ChannelModel
                {
                    Type = channel.Type,
                    ChannelId = channel.Id,
                    ChannelName = channelName,
                    Avatar = avatar,
                    NoReadCount = noReads.Find(n => n.ChannelId == channel.Id)?.Count ?? 0,
                });
            }
        }

        private async Task InitialMessage()
        {
            var selectedChannel = _selectedValue as ChannelModel;
            if (selectedChannel != null)
            {
                var channel = Channels.First(c => c.ChannelId == selectedChannel.ChannelId);
                _updateNoCountEventHandler.NotifyStateChanged(new UpdateNoCountEvent
                {
                    Type = UpdateNoCountEventType.Sub,
                    Count = channel.NoReadCount
                });
                channel.NoReadCount = 0;

                using var channelDbContext = _chatDbFactory.CreateDbContext(selectedChannel.ChannelId);
                MessageModels = channelDbContext.ChatMessages.OrderBy(m => m.Id).Take(100).Select(c => new MessageModel
                {
                    MessageId = c.Id,
                    SenderId = c.SenderId,
                    CreatedTime = c.CreatedTime,
                    Content = c.Content,
                }).AsNoTracking().ToList();

                using var adminDbContext = _dbFactory.CreateDbContext();
                UserModels = (from cm in adminDbContext.ChatChannelMembers
                              join u in adminDbContext.Users on cm.MemberId equals u.Id
                              where cm.ChatChannelId == selectedChannel.ChannelId
                              select new UserModel
                              {
                                  UserId = u.Id,
                                  RealName = u.RealName,
                                  Avatar = u.Avatar,
                              }).AsNoTracking().ToList();

                var state = await _stateProvider.GetAuthenticationStateAsync();
                MessageModels.ForEach(m => m.IsCurrentUserSend = m.SenderId == state.User.GetUserId());

                var noReadMsg = adminDbContext.ChatMessageNoReads.FirstOrDefault(mnr =>
                    mnr.ChannelId == selectedChannel.ChannelId && mnr.ReciverId == state.User.GetUserId());
                noReadMsg!.Count = 0;
                adminDbContext.ChatMessageNoReads.Update(noReadMsg);
                await adminDbContext.SaveChangesAsync();
            }
        }

        private record ChannelModel
        {
            public string? Avatar { get; set; }

            public int Type { get; set; }

            public int ChannelId { get; set; }

            public string? ChannelName { get; set; }

            public string? Caption { get; set; }

            public int NoReadCount { get; set; }
        }

        private record MessageModel
        {
            public int MessageId { get; set; }

            public int SenderId { get; set; }

            public DateTime CreatedTime { get; set; }

            public string? Content { get; set; }

            public bool IsCurrentUserSend { get; set; }
        }

        public record UserModel
        {
            public int UserId { get; set; }

            public string RealName { get; set; } = null!;

            public string? Avatar { get; set; }
        }
    }
}
