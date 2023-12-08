using BlazorAdmin.Core.Helper;
using BlazorAdmin.Core.Resources;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Component.Dialogs
{
    public partial class ConfirmUserPasswordDialog
    {
        private Dictionary<string, object> _inputAttributes  =
            new Dictionary<string, object>()
                {
                   { "autocomplete", "new-password2" },
                };

        private bool _isLoading = false;

        private PasswordModel _passwordModel = new PasswordModel();

        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        [Inject] public AuthenticationStateProvider? AuthStateProvider { get; set; }

        [Parameter] public EventCallback<CommonDialogEventArgs> ConfirmCallBack { get; set; }

        private async Task ConfirmPassword()
        {
            _isLoading = true;
            using var context = await _dbFactory.CreateDbContextAsync();

            var userState = await AuthStateProvider!.GetAuthenticationStateAsync();
            var user = context.Users.Find(userState.User.GetUserId());
            if (user == null)
            {
                _snackbarService.Add(_loc["NotFindUser"], Severity.Error);
                MudDialog?.Close(DialogResult.Cancel());
                return;
            }
            var isPwdValid = HashHelper.VerifyPassword(user.PasswordHash, _passwordModel.Password!);
            if (!isPwdValid)
            {
                _snackbarService.Add(_loc["PasswordValidFail"], Severity.Error);
                MudDialog?.Close(DialogResult.Cancel());
                return;
            }

            await ConfirmCallBack.InvokeAsync(new CommonDialogEventArgs());
            _isLoading = false;
            MudDialog?.Close(DialogResult.Ok(true));
        }

        private class PasswordModel
        {
            [Required(ErrorMessageResourceName = "Login_PasswordHelpText",
                ErrorMessageResourceType = typeof(CusCulture))]
            public string? Password { get; set; }
        }

    }
}
