using BlazorAdmin.Core.Extension;
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

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            using var context = await _dbFactory.CreateDbContextAsync();
            var user = await _stateProvider.GetAuthenticationStateAsync();
            _user = context.Users.Find(user.User.GetUserId());
        }

        private async Task ChangePwd()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            await _dialogService.Show<ChangePasswordDialog>(string.Empty, parameters, options).Result;
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
                    var user = await _stateProvider.GetAuthenticationStateAsync();
                    var findUser = context.Users.Find(user.User.GetUserId());
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
                var user = await _stateProvider.GetAuthenticationStateAsync();
                var findUser = context.Users.Find(user.User.GetUserId());
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
            var result = await _dialogService.Show<AvatarEditDialog>("编辑图片", parameters, options).Result;
            if (!result.Canceled)
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var user = await _stateProvider.GetAuthenticationStateAsync();
                _user = context.Users.Find(user.User.GetUserId());
                StateHasChanged();
            }
        }
    }
}
