using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Pages.Dialogs.Role
{
	public partial class DeleteRoleDialog
	{

		[Parameter] public int RoleId { get; set; }

		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }

		private async Task ConfirmDelete()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var role = context.Roles.Find(RoleId);
			if (role != null)
			{
				context.Roles.Remove(role);

				var userRoles = context.UserRoles.Where(ur => ur.RoleId == RoleId);
				context.UserRoles.RemoveRange(userRoles);

				var roleMenus = context.RoleMenus.Where(rm => rm.RoleId == RoleId);
				context.RoleMenus.RemoveRange(roleMenus);

				await context.CustomSaveChangesAsync(_stateProvider);
				_snackbarService.Add("删除成功！", Severity.Success);
				MudDialog?.Close(DialogResult.Ok(true));
			}
			else
			{
				_snackbarService.Add("角色信息不存在！", Severity.Error);
			}
		}
	}
}
