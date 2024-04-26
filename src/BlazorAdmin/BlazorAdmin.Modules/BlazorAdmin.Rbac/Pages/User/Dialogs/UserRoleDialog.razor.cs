using BlazorAdmin.Data.Entities.Rbac;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BlazorAdmin.Rbac.Pages.User.Dialogs
{
    public partial class UserRoleDialog
    {

        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        [Parameter] public int UserId { get; set; }

        private List<Data.Entities.Rbac.Role> RoleList = new();

        private Dictionary<int, bool> CheckedDic = new Dictionary<int, bool>();

        protected override async Task OnInitializedAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            RoleList = await context.Roles.Where(r => r.IsEnabled && !r.IsDeleted).ToListAsync();

            var userRoles = await context.UserRoles.Where(ur => ur.UserId == UserId).ToListAsync();

            foreach (var role in RoleList)
            {
                CheckedDic[role.Id] = userRoles.Any(ur => ur.RoleId == role.Id);
            }
        }

        private async Task SubmitUserRole()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var userRoles = await context.UserRoles.Where(ur => ur.UserId == UserId).ToListAsync();

            var deleteRoles = userRoles.Where(ur => !CheckedDic[ur.RoleId]);
            context.UserRoles.RemoveRange(deleteRoles);
            var addRoles = CheckedDic.Where(kv => userRoles.All(ur => kv.Key != ur.RoleId) && kv.Value);
            context.UserRoles.AddRange(addRoles.Select(kv => new UserRole
            {
                UserId = UserId,
                RoleId = kv.Key,
            }));

            await context.SaveChangesAsync();

            _snackbarService.Add("提交成功！", Severity.Success);
            MudDialog?.Close(DialogResult.Ok(true));

        }
    }
}
