﻿using BlazorAdmin.Component.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Servers.Core.Components.Dialogs
{
    public partial class CommonDeleteDialog
    {
        [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }

        [Parameter] public string? Title { get; set; }

        [Parameter] public string? ConfirmButtonText { get; set; }

        [Parameter] public EventCallback<CommonDialogEventArgs> ConfirmCallBack { get; set; }

        private bool _isLoading = false;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Title ??= _loc["CommonDeleteDialogTitle"];
            ConfirmButtonText ??= _loc["CommonDeleteDialogConfirmButtonText"];
        }

        private async Task ConfirmDelete()
        {
            _isLoading = true;
            await ConfirmCallBack.InvokeAsync(new CommonDialogEventArgs());
            _isLoading = false;
            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}
