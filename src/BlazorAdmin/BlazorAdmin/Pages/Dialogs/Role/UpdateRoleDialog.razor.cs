using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;

namespace BlazorAdmin.Pages.Dialogs.Role
{
	public partial class UpdateRoleDialog
	{
		private Dictionary<string, object> InputAttributes { get; set; } =
			new Dictionary<string, object>()
				{
				   { "autocomplete", "off2" },
				};

		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }

		[Parameter] public int RoleId { get; set; }

		private RoleUpdateModel RoleModel = new();

		protected override async Task OnInitializedAsync()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var role = context.Roles.Find(RoleId);
			if (role != null)
			{
				RoleModel = new RoleUpdateModel
				{
					Id = role.Id,
					RoleName = role.Name,
				};
			}
			else
			{
				_snackbarService.Add("角色不存在！", Severity.Error);
			}
		}

		private async Task UpdateSumbit()
		{
			using var context = _dbFactory.CreateDbContext();
			if (context.Roles.Any(u => u.Name == RoleModel.RoleName && u.Id != RoleModel.Id))
			{
				_snackbarService.Add("角色名称重复！", Severity.Error);
				return;
			}

			var role = context.Roles.Find(RoleId);
			if (role != null)
			{
				role.Name = RoleModel.RoleName!;
				await context.SaveChangesAsync();
				_snackbarService.Add("更新成功！", Severity.Success);
				MudDialog?.Close(DialogResult.Ok(true));
			}
			else
			{
				_snackbarService.Add("角色信息不存在！", Severity.Error);
			}
		}

		private record RoleUpdateModel
		{
			public int Id { get; set; }

			[Required(ErrorMessage = "请输入角色名称")]
			[MaxLength(200, ErrorMessage = "角色名称位数过长")]
			public string? RoleName { get; set; }
		}
	}
}
