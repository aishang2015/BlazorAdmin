using BlazorAdmin.Component.Dialogs;
using BlazorAdmin.Component.Pages;
using BlazorAdmin.Rbac.Pages.Role.Dialogs;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BlazorAdmin.Rbac.Pages.Role
{
    public partial class Role
    {
        private int Page = 1;

        private int Size = 10;

        private int Total = 0;

        private string? SearchText;

        private MudDataGrid<RoleModel> dataGridRef = null!;

        private List<RoleModel> Roles = new();

        private PageDataGridOne PageDataGridOne = new();

        private async Task InitialData()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            IQueryable<Data.Entities.Rbac.Role> query = context.Roles.Where(r => !r.IsDeleted);
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(u => u.Name!.Contains(SearchText));
            }

            Roles = await query.Select(r => new RoleModel
            {
                Id = r.Id,
                Name = r.Name,
                IsEnabled = r.IsEnabled
            }).Skip((Page - 1) * Size).Take(Size).ToListAsync();
            Total = await query.CountAsync();

            foreach (var role in Roles)
            {
                role.Number = (Page - 1) * Size + Roles.IndexOf(role) + 1;
            }
        }

        private async Task<GridData<RoleModel>> GetTableData(GridState<RoleModel> gridState)
        {
            await InitialData();
            return new GridData<RoleModel>() { TotalItems = Total, Items = Roles };
        }

        private async Task ChangeRoleActive(int roleId, bool isEnabled)
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var role = context.Roles.Find(roleId);
            if (role != null)
            {
                role.IsEnabled = isEnabled;
                await context.SaveChangesAsync();
                _snackbarService.Add(Loc["RolePage_StatusChangedMessage"], Severity.Success);
                Roles.FirstOrDefault(u => u.Id == roleId)!.IsEnabled = isEnabled;
            }
        }

        private async Task AddRoleClick()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var result = await _dialogService.Show<CreateRoleDialog>(Loc["RolePage_CreateNewTitle"], parameters, options).Result;
            if (!result.Canceled)
            {
                await dataGridRef.ReloadServerData();
            }
        }

        private async Task DeleteRoleClick(int roleId)
        {
            var isConfirmed = await _dialogService.ShowConfirmUserPasswodDialog();
            if (!isConfirmed)
            {
                return;
            }

            await _dialogService.ShowDeleteDialog(Loc["RolePage_DeleteTitle"], null,
                async (e) =>
                {
                    using var context = await _dbFactory.CreateDbContextAsync();
                    var role = context.Roles.Find(roleId);
                    if (role != null)
                    {
                        role.IsDeleted = true;
                        context.Roles.Update(role);

                        var userRoles = context.UserRoles.Where(ur => ur.RoleId == roleId);
                        context.UserRoles.RemoveRange(userRoles);

                        var roleMenus = context.RoleMenus.Where(rm => rm.RoleId == roleId);
                        context.RoleMenus.RemoveRange(roleMenus);

                        await context.SaveChangesAsync();
                        _snackbarService.Add("删除成功！", Severity.Success);
                    }
                    else
                    {
                        _snackbarService.Add("角色信息不存在！", Severity.Error);
                    }

                    await dataGridRef.ReloadServerData();
                });
        }

        private async Task EditRoleClick(int roleId)
        {
            var parameters = new DialogParameters
            {
                {"RoleId",roleId }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var result = await _dialogService.Show<UpdateRoleDialog>(Loc["RolePage_EditTitle"], parameters, options).Result;
            if (!result.Canceled)
            {
                await dataGridRef.ReloadServerData();
            }
        }

        private async Task SetRoleMenuClick(int roleId)
        {
            var parameters = new DialogParameters
            {
                {"RoleId",roleId }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            var result = await _dialogService.Show<RoleMenuDialog>(Loc["RolePage_SetRoleMenuTitle"], parameters, options).Result;
            if (!result.Canceled)
            {
                await dataGridRef.ReloadServerData();
            }
        }

        private async Task PageChangedClick(int page)
        {
            Page = page;
            await dataGridRef.ReloadServerData();
        }

        private void SearchReset()
        {
            SearchText = null;
            Page = 1;
            dataGridRef.ReloadServerData();
        }

        private class RoleModel
        {
            public int Id { get; set; }

            public int Number { get; set; }

            public string? Name { get; set; }

            public bool IsEnabled { get; set; }
        }
    }
}
