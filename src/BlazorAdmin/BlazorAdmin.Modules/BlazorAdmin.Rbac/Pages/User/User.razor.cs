using BlazorAdmin.Component.Dialogs;
using BlazorAdmin.Component.Pages;
using BlazorAdmin.Core.Extension;
using BlazorAdmin.Rbac.Pages.User.Dialogs;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using BlazorAdmin.Data.Extensions;
using MudBlazor;
using static BlazorAdmin.Component.Pages.PagePagination;

namespace BlazorAdmin.Rbac.Pages.User
{
    public partial class User
    {
        private List<UserModel> Users = new List<UserModel>();

        private SearchObject searchObject = new();

        private MudDataGrid<UserModel> dataGrid = null!;

        private PageDataGridConfig PageDataGridOne = new();

        private List<Data.Entities.Rbac.Role> RoleList = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            using var context = await _dbFactory.CreateDbContextAsync();
            RoleList = context.Roles.AsNoTracking().ToList();
        }


        private async Task<GridData<UserModel>> GetTableData(GridState<UserModel> gridState)
        {
            await InitialData();
            return new GridData<UserModel>() { TotalItems = Users.Count, Items = Users };
        }


        private async Task InitialData()
        {
            using var context = await _dbFactory.CreateDbContextAsync();

            var searchedUserIdList = new List<int>();
            if (!string.IsNullOrEmpty(searchObject.SearchRole))
            {
                var role = int.Parse(searchObject.SearchRole);
                searchedUserIdList = context.UserRoles.Where(ur => ur.RoleId == role)
                    .Select(ur => ur.UserId).Distinct().ToList();
            }

            if (searchObject.SelectedOrganization != null)
            {
                var orgs = context.Organizations.GetAllSubOrganiations(searchObject.SelectedOrganization.Value)
                    .Select(org => org.Id);

                var userIds = context.OrganizationUsers.Where(ou => orgs.Contains(ou.OrganizationId))
                   .Select(ou => ou.UserId).Distinct().ToList();
                searchedUserIdList.AddRange(userIds);
            }

            var query = context.Users.Where(u => !u.IsDeleted && !u.IsSpecial)
                .AndIfExist(searchObject.SearchText, u => u.Name.Contains(searchObject.SearchText!))
                .AndIfExist(searchObject.SearchRealName, u => u.RealName.Contains(searchObject.SearchRealName!))
                .AndIf(searchObject.SelectedOrganization != null, u => searchedUserIdList.Contains(u.Id));

            Users = await query.Skip((searchObject.Page - 1) * searchObject.Size).Take(searchObject.Size)
                .Select(p => new UserModel
                {
                    Id = p.Id,
                    Avatar = p.Avatar,
                    Name = p.Name,
                    RealName = p.RealName,
                    IsEnabled = p.IsEnabled,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber
                }).ToListAsync();
            searchObject.Total = await query.CountAsync();
            StateHasChanged();

            var roles = (from r in context.Roles
                         join ur in context.UserRoles on r.Id equals ur.RoleId
                         select new { r.Name, ur.UserId }).ToList();


            var organizations = (from r in context.Organizations
                                 join ou in context.OrganizationUsers on r.Id equals ou.OrganizationId
                                 select new { r.Name, ou.UserId }).ToList();

            foreach (var user in Users)
            {
                user.Number = (searchObject.Page - 1) * searchObject.Size + Users.IndexOf(user) + 1;
                user.Roles = roles.Where(r => r.UserId == user.Id).Select(r => r.Name).ToList();
                user.Organizations = organizations.Where(o => o.UserId == user.Id).Select(o => o.Name).ToList();
            }
        }

        private async Task PageChangedClick(int page)
        {
            searchObject.Page = page;
            await dataGrid.ReloadServerData();
        }

        private async Task AddUserClick()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var dialog = await _dialogService.ShowAsync<CreateUserDialog>(Loc["UserPage_CreateNewTitle"], parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await dataGrid.ReloadServerData();
            }
        }

        private async Task DeleteUserClick(int userId)
        {
            await _dialogService.ShowDeleteDialog(Loc["UserPage_DeleteTitle"], null,
            async (e) =>
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var user = context.Users.Find(userId);
                if (user != null)
                {
                    user.IsDeleted = true;
                    context.Users.Update(user);

                    var urs = context.UserRoles.Where(ur => ur.UserId == userId);
                    context.UserRoles.RemoveRange(urs);

                    await context.SaveChangesAsync();

                    _snackbarService.Add("删除成功！", Severity.Success);
                }
                else
                {
                    _snackbarService.Add("用户信息不存在！", Severity.Error);
                }
                await dataGrid.ReloadServerData();
            });
        }


        private async Task EditUserClick(int userId)
        {
            var parameters = new DialogParameters
            {
                {"UserId",userId }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var dialog = await _dialogService.ShowAsync<UpdateUserDialog>(Loc["UserPage_EditTitle"], parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await dataGrid.ReloadServerData();
            }
        }

        private async Task ChangePasswordClick(int userId)
        {
            var parameters = new DialogParameters
            {
                {"UserId",userId }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var dialog = await _dialogService.ShowAsync<ChangePasswordDialog>(Loc["UserPage_ModifyPasswordTitle"], parameters, options);
            await dialog.Result;
        }

        private async Task SetUserRoleClick(int userId)
        {
            var parameters = new DialogParameters
            {
                {"UserId",userId }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var dialog = await _dialogService.ShowAsync<UserRoleDialog>(Loc["UserPage_SetUserRoleTitle"], parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await dataGrid.ReloadServerData();
            }
        }

        private async Task ChangeUserActive(int userId, bool isEnabled)
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var user = context.Users.Find(userId);
            if (user != null)
            {
                user.IsEnabled = isEnabled;
                await context.SaveChangesAsync();
                _snackbarService.Add(Loc["UserPage_StatusChangedMessage"], Severity.Success);
                Users.FirstOrDefault(u => u.Id == userId)!.IsEnabled = isEnabled;
            }
        }


        private void SearchReset()
        {
            searchObject = new();
            searchObject.Page = 1;
            dataGrid.ReloadServerData();
        }


        private record SearchObject : PaginationModel
        {
            public string? SearchText { get; set; }

            public string? SearchRole { get; set; }

            public string? SearchRealName { get; set; }

            public int? SelectedOrganization { get; set; }
        }

        private class UserModel
        {
            public int Id { get; set; }

            public int Number { get; set; }

            public string? Avatar { get; set; }

            public string Name { get; set; } = null!;

            public string? RealName { get; set; }

            public string? Email { get; set; }

            public string? PhoneNumber { get; set; }

            public bool IsEnabled { get; set; }

            public List<string> Roles { get; set; } = new();

            public List<string> Organizations { get; set; } = new();
        }
    }
}
