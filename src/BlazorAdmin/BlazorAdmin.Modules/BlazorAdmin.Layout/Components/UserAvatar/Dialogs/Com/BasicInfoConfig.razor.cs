using BlazorAdmin.Core.Auth;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using User = BlazorAdmin.Data.Entities.Rbac.User;

namespace BlazorAdmin.Layout.Components.UserAvatar.Dialogs.Com
{
    public partial class BasicInfoConfig
    {

        private User _user = new();

        private bool _isEditUserName = false;

        private string _editUserName = string.Empty;

        private bool _isEditRealName = false;

        private string _editRealName = string.Empty;

        private bool _isEditEmail = false;

        private string _editEmail = string.Empty;

        private bool _isEditPhone = false;

        private string _editPhone = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            using var context = await _dbFactory.CreateDbContextAsync();
            var userId = await _stateProvider.GetUserIdAsync();
            _user = context.Users.Find(userId);
        }

        private async Task ChangePwd()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            var dialog = await _dialogService.ShowAsync<ChangePasswordDialog>(string.Empty, parameters, options);
            await dialog.Result;
        }


        private void EditUserName()
        {
            _isEditUserName = true;
            _editUserName = _user.Name;
        }

        private async Task EditUserNameBlur()
        {
            if (string.IsNullOrEmpty(_editUserName))
            {
                _snackbarService.Add("用户名不能为空！", Severity.Error);
                return;
            }


            if (_editUserName == _user.Name)
            {
                _isEditUserName = false;
            }
            else
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                if (context.Users.Any(u => u.Name == _editUserName))
                {
                    _snackbarService.Add("此用户名已有人使用，请修改！", Severity.Error);
                }
                else
                {
                    var userid = await _stateProvider.GetUserIdAsync();
                    var findUser = context.Users.Find(userid);
                    findUser.Name = _editUserName;
                    await context.SaveChangesAsync();
                    _user.Name = _editUserName;
                    _isEditUserName = false;
                    _snackbarService.Add("用户名修改成功！", Severity.Success);
                }

            }
        }

        private void EditRealName()
        {
            _isEditRealName = true;
            _editRealName = _user.RealName;
        }

        private async Task EditRealNameBlur()
        {
            if (string.IsNullOrEmpty(_editRealName))
            {
                _snackbarService.Add("姓名不能为空！", Severity.Error);
                return;
            }

            if (_editRealName == _user.RealName)
            {
                _isEditRealName = false;
            }
            else
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var userId = await _stateProvider.GetUserIdAsync();
                var findUser = context.Users.Find(userId);
                findUser.RealName = _editRealName;
                await context.SaveChangesAsync();
                _user.RealName = _editRealName;
                _isEditRealName = false;
                _snackbarService.Add("用户名修改成功！", Severity.Success);
            }
        }

        private async Task UploadFiles(IBrowserFile file)
        {
            var parameters = new DialogParameters
            {
                {"BrowserFile",file }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = false };
            var dialog = await _dialogService.ShowAsync<AvatarEditDialog>("编辑图片", parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var userId = await _stateProvider.GetUserIdAsync();
                _user = context.Users.Find(userId);
                StateHasChanged();
            }
        }

        private void EditEmail()
        {
            _isEditEmail = true;
            _editEmail = _user.Email ?? string.Empty;
        }

        private async Task EditEmailBlur()
        {
            if (!string.IsNullOrEmpty(_editEmail) && !System.Text.RegularExpressions.Regex.IsMatch(_editEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                _snackbarService.Add("请输入正确的邮箱格式！", Severity.Error);
                return;
            }

            if (_editEmail == _user.Email)
            {
                _isEditEmail = false;
            }
            else
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var userId = await _stateProvider.GetUserIdAsync();
                var findUser = context.Users.Find(userId);
                findUser.Email = _editEmail;
                await context.SaveChangesAsync();
                _user.Email = _editEmail;
                _isEditEmail = false;
                _snackbarService.Add("邮箱修改成功！", Severity.Success);
            }
        }

        private void EditPhone()
        {
            _isEditPhone = true;
            _editPhone = _user.PhoneNumber ?? string.Empty;
        }

        private async Task EditPhoneBlur()
        {
            if (!string.IsNullOrEmpty(_editPhone) && !System.Text.RegularExpressions.Regex.IsMatch(_editPhone, @"^1[3-9]\d{9}$"))
            {
                _snackbarService.Add("请输入正确的手机号格式！", Severity.Error);
                return;
            }

            if (_editPhone == _user.PhoneNumber)
            {
                _isEditPhone = false;
            }
            else
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var userId = await _stateProvider.GetUserIdAsync();
                var findUser = context.Users.Find(userId);
                findUser.PhoneNumber = _editPhone;
                await context.SaveChangesAsync();
                _user.PhoneNumber = _editPhone;
                _isEditPhone = false;
                _snackbarService.Add("手机号修改成功！", Severity.Success);
            }
        }
    }
}
