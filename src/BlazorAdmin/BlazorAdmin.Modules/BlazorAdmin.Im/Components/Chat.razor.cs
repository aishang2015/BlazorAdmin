using BlazorAdmin.Core.Auth;
using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data.Constants;
using BlazorAdmin.Im.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;

namespace BlazorAdmin.Im.Components
{
    public partial class Chat
    {
        private bool _isDialogOpen = false;

        private bool _haveMsg = false;

        private int _noReadCount = 0;

        private HubConnection? _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await RefreshNoReadCountAsync();
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_navManager.BaseUri.TrimEnd('/') + ChatHub.ChatHubUrl,
                        options => options.AccessTokenProvider = async () =>
                        {
                            var cookieUtil = await _jsRuntime.InvokeAsync<IJSObjectReference>
                                ("import", "./js/cookieUtil.js");
                            var token = await cookieUtil.InvokeAsync<string>("getCookie", CommonConstant.UserToken);
                            return token;
                        })
                    .Build();
                await _hubConnection.StartAsync();

                RegistSignalrMethod();

                StateHasChanged();
            }
        }

        private async Task RefreshNoReadCountAsync()
        {
            var userId = await _stateProvider.GetUserIdAsync();
            using var context = _dbFactory.CreateDbContext();

            _noReadCount = context.NotReadedMessages
                .Where(m => m.UserId == userId)
                .Count();
        }

        private void RegistSignalrMethod()
        {
            _hubConnection?.On<ChatMessageReceivedModel>("ReceiveMessage", async (model) =>
            {
                _noReadCount += 1;
                await InvokeAsync(() => StateHasChanged());
            });
        }

        private void UnRegistSignalrMethod()
        {
            _hubConnection?.Remove("ReceiveMessage");
        }

        private async Task ViewIm()
        {
            UnRegistSignalrMethod();
            _isDialogOpen = true;
            var countChangedCallback = new EventCallback<int>(null,
                 (int number) =>
                 {
                     CountChanged(number);
                 });
            var parameters = new DialogParameters {
                {"Connection", _hubConnection },
                {"NoReadCountChanged", countChangedCallback }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            var result = await _dialogService.Show<ChatDialog>(null, parameters, options).Result;
            await RefreshNoReadCountAsync();
            _isDialogOpen = false;
            RegistSignalrMethod();
        }

        private void CountChanged(int count)
        {
            _noReadCount = count;
            InvokeAsync(() => StateHasChanged());
        }

        public async ValueTask DisposeAsync()
        {
            //await _hubConnection?.StopAsync();
            //await _hubConnection?.DisposeAsync();
        }
    }
}
