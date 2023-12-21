using BlazorAdmin.Core.Chat;
using BlazorAdmin.Data.Constants;
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

            var state = await _stateProvider.GetAuthenticationStateAsync();
            using var context = _dbFactory.CreateDbContext();
            _noReadCount = context.ChatMessageNoReads.Where(n => n.ReciverId == state.User.GetUserId()).Sum(n => n.Count);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navManager.BaseUri.TrimEnd('/') + ChatHub.ChatHubUrl,
                    options => options.AccessTokenProvider = async () =>
                    {
                        var result = await _localStorage.GetAsync<string>(CommonConstant.UserToken);
                        return result.Value;
                    })
                .Build();
            await _hubConnection.StartAsync();

             StateHasChanged();
        }

        private async Task ViewIm()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            var result = await _dialogService.Show<ChatDialog>(null, parameters, options).Result;
        }

        public async ValueTask DisposeAsync()
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}
