using BlazorAdmin.Core.Extension;
using BlazorAdmin.Core.Helper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Layout.Components.UserAvatar.Dialogs.Com
{
    public partial class ChangePasswordDialog
    {
        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        private PasswordChangeModel PasswordModel = new();

        private async Task Submit()
        {
            using var context = _dbFactory.CreateDbContext();
            var userState = await _stateProvider.GetAuthenticationStateAsync();
            var user = context.Users.Find(userState.User.GetUserId());
            if (user != null)
            {
                user.PasswordHash = HashHelper.HashPassword(PasswordModel.Password!);
                await context.AuditSaveChangesAsync();
                _snackbarService.Add("密码修改成功！", Severity.Success);
                MudDialog?.Close(DialogResult.Ok(true));
            }
            else
            {
                _snackbarService.Add("用户信息不存在！", Severity.Error);
            }
        }

        private record PasswordChangeModel
        {
            [Required(ErrorMessage = "请输入密码")]
            [MinLength(4, ErrorMessage = "密码位数过短")]
            [MaxLength(100, ErrorMessage = "密码位数过长")]
            [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$", ErrorMessage = "密码中必须包含大小写字母以及数字")]
            public string? Password { get; set; }

            [Compare(nameof(Password), ErrorMessage = "两次输入的密码不一致")]
            public string? ConfirmPassword { get; set; }
        }
    }
}
