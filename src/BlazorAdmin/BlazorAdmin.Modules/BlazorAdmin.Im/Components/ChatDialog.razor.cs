using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data.Entities.Chat;
using BlazorAdmin.Im.Events;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using MudBlazor;

namespace BlazorAdmin.Im.Components
{
    public partial class ChatDialog
    {
        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        [Parameter] public HubConnection Connection { get; set; } = null!;

        [Parameter] public EventCallback<int> NoReadCountChanged { get; set; }

        private string _textValue = string.Empty;

        private string _messageValue = string.Empty;

        private object? _selectedValue;

        private MudListItem? _selectedItem;

        private List<ChannelModel> Channels = new List<ChannelModel>();

        private List<ChannelModel> AllChannels = new List<ChannelModel>();

        private List<MessageModel> MessageModels = new List<MessageModel>();

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

            await NoReadCountChanged.InvokeAsync(AllChannels.Sum(c => c.NoReadCount));

            // signalR订阅
            Connection.On<ChatMessageReceivedModel>("ReceiveMessage", async (model) =>
            {
                if (model.ChannelId == (_selectedValue as ChannelModel).ChannelId)
                {
                    var state = await _stateProvider.GetAuthenticationStateAsync();
                    MessageModels.Add(new MessageModel
                    {
                        Content = model.Content,
                        CreatedTime = DateTime.Now,
                        SenderId = model.SenderId,
                        IsCurrentUserSend = model.SenderId == state.User.GetUserId()
                    });
                }
                else
                {
                    AllChannels.First(c => c.ChannelId == model.ChannelId).NoReadCount++;
                    await NoReadCountChanged.InvokeAsync(AllChannels.Sum(c => c.NoReadCount));
                }
                await InvokeAsync(() => StateHasChanged());
            });
        }

        private async Task SelectedChannelChanged(object selectedValue)
        {
            _selectedValue = selectedValue;
            await InitialMessage();
            await NoReadCountChanged.InvokeAsync(AllChannels.Sum(c => c.NoReadCount));
        }

        private async Task InitialChannelList()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();

            using var context = await _dbFactory.CreateDbContextAsync();

            // 频道id列表
            var channelIdList = context.ChatChannelMembers.Where(ccm => ccm.MemberId == state.User.GetUserId())
                .Select(c => c.ChatChannelId).ToList();

            // 频道信息
            var channelModelList = (from c in context.ChatChannels
                                    where channelIdList.Contains(c.Id)
                                    select new ChannelModel
                                    {
                                        ChannelId = c.Id,
                                        Name = c.Name,
                                        NoReadCount = context.ChatChannelMembers
                                             .First(ccm => ccm.ChatChannelId == c.Id && ccm.MemberId == state.User.GetUserId())
                                             .NotReadCount,
                                        UserList = context.ChatChannelMembers
                                             .Where(ccm => ccm.ChatChannelId == c.Id)
                                             .Select(ccm => new UserModel
                                             {
                                                 UserId = ccm.MemberId
                                             }).ToList()
                                    }).ToList();

            // 用户id
            var userIdList = channelModelList.SelectMany(c => c.UserList).Select(c => c.UserId).Distinct().ToList();
            var userList = context.Users.Where(u => userIdList.Contains(u.Id));
            foreach (var channel in channelModelList)
            {
                foreach (var user in channel.UserList)
                {
                    var findUser = userList.FirstOrDefault(u => u.Id == user.UserId);
                    user.Avatar = findUser?.Avatar;
                    user.RealName = findUser?.RealName;
                }

                if (channel.UserList.Count == 2)
                {
                    channel.Avatar = channel.UserList.First(u => u.UserId != state.User.GetUserId()).Avatar;
                    channel.Name = channel.UserList.First(u => u.UserId != state.User.GetUserId()).RealName;
                }
            }
            AllChannels = channelModelList;
            Channels = channelModelList;
        }

        private async Task InitialMessage()
        {
            var selectedChannel = _selectedValue as ChannelModel;
            if (selectedChannel != null)
            {
                // 清理页面未读数
                var channel = Channels.First(c => c.ChannelId == selectedChannel.ChannelId);
                channel.NoReadCount = 0;

                // 取聊天记录
                using var channelDbContext = _chatDbFactory.CreateDbContext(selectedChannel.ChannelId);
                MessageModels = channelDbContext.ChatMessages.OrderBy(m => m.Id).Take(100).Select(c => new MessageModel
                {
                    MessageId = c.Id,
                    SenderId = c.SenderId,
                    CreatedTime = c.CreatedTime,
                    Content = c.Content,
                }).AsNoTracking().ToList();


                // 判断是发送者
                var state = await _stateProvider.GetAuthenticationStateAsync();
                MessageModels.ForEach(m => m.IsCurrentUserSend = m.SenderId == state.User.GetUserId());

                // 更新未读数量
                using var adminDbContext = _dbFactory.CreateDbContext();
                var noReadMsg = adminDbContext.ChatChannelMembers.FirstOrDefault(mnr =>
                    mnr.ChatChannelId == selectedChannel.ChannelId && mnr.MemberId == state.User.GetUserId());
                noReadMsg!.NotReadCount = 0;
                adminDbContext.ChatChannelMembers.Update(noReadMsg);
                await adminDbContext.SaveChangesAsync();
            }
        }

        private async Task SelectUserToChat()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            var result = await _dialogService.Show<UserPickerDialog>(null, parameters, options).Result;
            if (!result.Canceled)
            {
                await InitialChannelList();
            }
        }

        private async Task SearchedTextChanged(string value)
        {
            _textValue = value;
            Channels = AllChannels.Where(c => c.Name.Contains(value)).ToList();
        }

        private async Task SendMessage()
        {
            var selectedChannel = _selectedValue as ChannelModel;
            var state = await _stateProvider.GetAuthenticationStateAsync();
            await _messageSender.SendChannelMessage(state.User.GetUserId(), selectedChannel.ChannelId, _messageValue);
            _messageValue = null;
        }

        public async ValueTask DisposeAsync()
        {
            Connection.Remove("ReceiveMessage");
        }

        private record ChannelModel
        {
            public string? Avatar { get; set; }

            public int ChannelId { get; set; }

            public string? Name { get; set; }

            public int NoReadCount { get; set; }

            public List<UserModel> UserList { get; set; } = new();
        }

        public record UserModel
        {
            public int UserId { get; set; }

            public string RealName { get; set; } = null!;

            public string? Avatar { get; set; }
        }

        private record MessageModel
        {
            public int MessageId { get; set; }

            public int SenderId { get; set; }

            public DateTime CreatedTime { get; set; }

            public string? Content { get; set; }

            public bool IsCurrentUserSend { get; set; }
        }
    }
}
