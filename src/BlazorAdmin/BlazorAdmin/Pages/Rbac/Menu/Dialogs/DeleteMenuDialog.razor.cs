using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BlazorAdmin.Pages.Rbac.Menu.Dialogs
{
	public partial class DeleteMenuDialog
	{
		[Parameter] public List<int> MenuIdList { get; set; } = new();

		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }


		private async Task ConfirmDelete()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			using var tran = context.Database.BeginTransaction();

			var count = await context.Menus.Where(m => MenuIdList.Contains(m.Id)).ExecuteDeleteAsync();
			await context.RoleMenus.Where(rm => MenuIdList.Contains(rm.MenuId)).ExecuteDeleteAsync();

			await tran.CommitAsync();

			if (count > 0)
			{
				_snackbarService.Add("删除成功！", Severity.Success);
				MudDialog?.Close(DialogResult.Ok(true));
			}
			else
			{
				_snackbarService.Add("菜单不存在！", Severity.Error);
			}
		}
	}
}
