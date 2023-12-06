using BlazorAdmin.Core.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Rbac.Pages.Role.Dialogs
{
	public partial class CreateRoleDialog
	{
		private Dictionary<string, object> InputAttributes { get; set; } =
			new Dictionary<string, object>()
				{
				   { "autocomplete", "off2" },
				};

		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }

		private RoleCreateModel RoleModel = new();

		private async Task CreateSubmit()
		{
			using var context = _dbFactory.CreateDbContext();
			if (context.Roles.Any(u => u.Name == RoleModel.RoleName))
			{
				_snackbarService.Add("角色名称重复！", Severity.Error);
				return;
			}

			context.Roles.Add(new Data.Entities.Role
			{
				Name = RoleModel.RoleName,
				IsEnabled = true
			});
			await context.AuditSaveChangesAsync();
			_snackbarService.Add("创建成功！", Severity.Success);
			MudDialog?.Close(DialogResult.Ok(true));
		}

		private record RoleCreateModel
		{
			[Required(ErrorMessage = "请输入角色名称")]
			[MaxLength(200, ErrorMessage = "角色名称位数过长")]
			public string? RoleName { get; set; }
		}
	}
}
