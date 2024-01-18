using BlazorAdmin.Component.Dialogs;
using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Constants;
using BlazorAdmin.Im.Events;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Im.Components
{
    public partial class Chat
    {
        private bool _haveMsg = false;

        private int _noReadCount = 0;

        private HubConnection _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navManager.BaseUri.TrimEnd('/') + ChatHub.ChatHubUrl,
                    options => options.AccessTokenProvider = async () =>
                    {
                        var result = await _localStorage.GetAsync<string>(CommonConstant.UserToken);
                        return result.Value;
                    })
                .Build();
            await _hubConnection.StartAsync();

            RegistSignalrMethod();

            await RefreshNoReadCountAsync();

            StateHasChanged();
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
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
