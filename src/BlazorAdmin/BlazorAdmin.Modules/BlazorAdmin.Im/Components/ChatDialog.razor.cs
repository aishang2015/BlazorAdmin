using BlazorAdmin.Core.Auth;
using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
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

        private string? _messageValue = string.Empty;


        private ItemModel? SelectedItem;
        private List<UserModel> SelectedChatUsers = new();
        private List<ItemModel> ItemModelList = new();
        private List<MessageModel> MessageModels = new List<MessageModel>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var scrollModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorAdmin.Im/js/scroll.js");
            await scrollModule.InvokeVoidAsync("scrollToBottom");
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitialChatItemList();
            await InitialChatItemMessages();

            // signalR订阅
            Connection.On<ChatMessageReceivedModel>("ReceiveMessage", async (model) =>
            {
                var userId = await _stateProvider.GetUserIdAsync();
                if (SelectedItem != null)
                {
                    if (SelectedItem.IsPrivate)
                    {
                        if (SelectedItem.Id == model.SenderId
                            || SelectedItem.Id == model.ReceiverId)
                        {
                            await InitialChatItemMessages();
                            return;
                        }
                    }
                    else
                    {
                        if (SelectedItem.Id == model.ChannelId)
                        {
                            await InitialChatItemMessages();
                            return;
                        }
                    }

                    await InitialChatItemList();
                }

                await InvokeAsync(() => StateHasChanged());
            });
        }

        private async Task SelectedItemChanged(ItemModel selectedValue)
        {
            SelectedItem = selectedValue;
            await InitialChatItemMessages();
        }

        private async Task InitialChatItemList()
        {
            var userId = await _stateProvider.GetUserIdAsync();

            using var context = await _dbFactory.CreateDbContextAsync();

            SelectedChatUsers = context.Users.Select(u => new UserModel
            {
                Avatar = u.Avatar,
                RealName = u.RealName,
                UserId = u.Id
            }).ToList();

            var noReadInfoList = context.NotReadedMessages
                .Where(m => m.UserId == userId)
                .AsNoTracking().ToList();

            var privateList = context.PrivateMessages
                .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                .Select(m => new { m.SenderId, m.ReceiverId })
                .Distinct().ToList();

            var privateUserList = privateList.Select(l => l.SenderId)
                .Concat(privateList.Select(l => l.ReceiverId))
                .Distinct().ToList();

            var privates = context.Users
                .Where(u => privateUserList.Contains(u.Id) && u.Id != userId)
                .Select(u => new ItemModel
                {
                    Avatar = u.Avatar,
                    Name = u.RealName,
                    IsPrivate = true,
                    Id = u.Id
                }).ToList();

            foreach (var privateUser in privates)
            {
                privateUser.NotReadedCount = noReadInfoList.Count(l => l.SendUserId == privateUser.Id);
            }

            var channelList = context.GroupMembers.Where(m => m.MemberId == userId)
                .Select(m => m.GroupId).Distinct().ToList();
            var groups = context.Groups.Where(g=>channelList.Contains(g.Id)).Select(g => new ItemModel
            {
                Avatar = null,
                Name = g.Name,
                IsPrivate = false,
                Id = g.Id
            }).ToList();

            foreach (var group in groups)
            {
                group.NotReadedCount = noReadInfoList.Count(l => l.GroupId == group.Id);
            }

            ItemModelList.Clear();
            ItemModelList.AddRange(privates);
            ItemModelList.AddRange(groups);

            if (SelectedItem == null && ItemModelList.Count > 0)
            {
                SelectedItem = ItemModelList.First();
            }
        }

        private async Task InitialChatItemMessages()
        {
            if (SelectedItem == null)
            {
                return;
            }

            using var context = await _dbFactory.CreateDbContextAsync();
            var userId = await _stateProvider.GetUserIdAsync();

            MessageModels.Clear();
            if (SelectedItem.IsPrivate)
            {
                MessageModels = context.PrivateMessages
                    .OrderBy(m => m.Id)
                    .Where(m => m.MessageType != 0)
                    .Where(m =>
                           (m.ReceiverId == SelectedItem.Id && m.SenderId == userId)
                        || (m.ReceiverId == userId && m.SenderId == SelectedItem.Id))
                    .Select(m => new MessageModel
                    {
                        MessageId = m.Id,
                        SenderId = m.SenderId,
                        Content = m.Content,
                        CreatedTime = m.SendTime,
                        IsCurrentUserSend = m.SenderId == userId
                    }).ToList();
            }
            else
            {
                MessageModels = context.GroupMessages
                    .OrderBy(m => m.Id)
                    .Where(m => m.MessageType != 0)
                    .Where(m => m.GroupId == SelectedItem.Id)
                    .Select(m => new MessageModel
                    {
                        MessageId = m.Id,
                        SenderId = m.SenderId,
                        Content = m.Content,
                        CreatedTime = m.SendTime,
                        IsCurrentUserSend = m.SenderId == userId

                    }).ToList();
            }

            if (SelectedItem.IsPrivate)
            {
                context.NotReadedMessages
                    .Where(n => n.UserId == userId && n.SendUserId == SelectedItem.Id)
                    .ExecuteDelete();
            }
            else
            {
                context.NotReadedMessages
                    .Where(n => n.UserId == userId && n.GroupId == SelectedItem.Id)
                    .ExecuteDelete();
            }
            SelectedItem.NotReadedCount = 0;
            await InvokeAsync(StateHasChanged);
        }

        private async Task SelectUserToChat()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            var result = await _dialogService.Show<UserPickerDialog>(null, parameters, options).Result;
            if (!result.Canceled)
            {
                await InitialChatItemList();
            }
        }

        private async Task SearchedTextChanged(string value)
        {
            _textValue = value;
            //Channels = AllChannels.Where(c => c.Name.Contains(value)).ToList();
        }

        private async Task SendMessage()
        {
            var userId = await _stateProvider.GetUserIdAsync();
            await _messageSender.SendChannelMessage(
                userId,
                SelectedItem!.IsPrivate ? null : SelectedItem.Id,
                !SelectedItem!.IsPrivate ? null : SelectedItem.Id,
                _messageValue);
            _messageValue = null;
        }

        public async ValueTask DisposeAsync()
        {
            Connection.Remove("ReceiveMessage");
        }

        private record ItemModel
        {
            public string? Avatar { get; set; }

            public string? Name { get; set; }

            public int Id { get; set; }

            public bool IsPrivate { get; set; }

            public int NotReadedCount { get; set; }
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
