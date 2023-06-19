using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Pages.Dialogs.User
{
	public partial class DeleteUserDialog
	{
		[Parameter] public int UserId { get; set; }

		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }


		private async Task ConfirmDelete()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var user = context.Users.Find(UserId);
			if (user != null)
			{
				context.Users.Remove(user);

				var urs = context.UserRoles.Where(ur => ur.UserId == UserId);
				context.UserRoles.RemoveRange(urs);

				await context.CustomSaveChangesAsync(_stateProvider);

				_snackbarService.Add("删除成功！", Severity.Success);
				MudDialog?.Close(DialogResult.Ok(true));
			}
			else
			{
				_snackbarService.Add("用户信息不存在！", Severity.Error);
			}
		}
	}
}
