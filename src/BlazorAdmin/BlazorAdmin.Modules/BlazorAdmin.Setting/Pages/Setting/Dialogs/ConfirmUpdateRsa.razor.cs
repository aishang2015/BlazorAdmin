﻿using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Setting.Pages.Setting.Dialogs
{
    public partial class ConfirmUpdateRsa
    {

        [CascadingParameter] IMudDialogInstance? MudDialog { get; set; }

        private void Confirm()
        {
            MudDialog?.Close(DialogResult.Ok(true));
        }
    }
}
