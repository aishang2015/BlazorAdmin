using BlazorAdmin.Core.Chat;
using BlazorAdmin.Data.Constants;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using MudBlazor;
using static MudBlazor.Colors;

namespace BlazorAdmin.Im.Components
{
    public partial class Chat
    {
        private bool _haveMsg = false;

        private int _noReadCount = 0;

        private HubConnection? _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
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

                await RefreshNoReadCountAsync();

                StateHasChanged();
            }
        }

        private async Task RefreshNoReadCountAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            using var context = _dbFactory.CreateDbContext();
            _noReadCount = context.ChatChannelMembers
                .Where(n => n.MemberId == state.User.GetUserId())
                .Sum(n => n.NotReadCount);
        }

        private void RegistSignalrMethod()
        {
            _hubConnection.On<ChatMessageReceivedModel>("ReceiveMessage", async (model) =>
            {
                _noReadCount += 1;
                await InvokeAsync(() => StateHasChanged());
            });
        }

        private void UnRegistSignalrMethod()
        {
            _hubConnection.Remove("ReceiveMessage");
        }

        private async Task ViewIm()
        {
            UnRegistSignalrMethod();
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
