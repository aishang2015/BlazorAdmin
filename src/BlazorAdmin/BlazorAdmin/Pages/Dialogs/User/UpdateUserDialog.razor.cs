using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace BlazorAdmin.Pages.Dialogs.User
{
	public partial class UpdateUserDialog
	{
		private Dictionary<string, object> InputAttributes { get; set; } =
			new Dictionary<string, object>()
				{
				   { "autocomplete", "off2" },
				};

		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }

		[Parameter] public int UserId { get; set; }

		private UserUpdateModel UserModel = new();

		protected override async Task OnInitializedAsync()
		{
			using var context = _dbFactory.CreateDbContext();
			var user = context.Users.Find(UserId);
			if (user != null)
			{
				UserModel = new UserUpdateModel
				{
					Id = user.Id,
					UserName = user.Name,
				};
			}
			else
			{
				_snackbarService.Add("用户信息不存在！", Severity.Error);
			}
		}

		private async Task UpdateSumbit()
		{
			using var context = _dbFactory.CreateDbContext();
			if (context.Users.Any(u => u.Name == UserModel.UserName && u.Id != UserModel.Id))
			{
				_snackbarService.Add("用户名重复！", Severity.Error);
				return;
			}

			var user = context.Users.Find(UserId);
			if (user != null)
			{
				user.Name = UserModel.UserName!;
				await context.SaveChangesAsync();
				_snackbarService.Add("更新成功！", Severity.Success);
				MudDialog?.Close(DialogResult.Ok(true));
			}
			else
			{
				_snackbarService.Add("用户信息不存在！", Severity.Error);
			}
		}

		private record UserUpdateModel
		{
			public int Id { get; set; }

			[Required(ErrorMessage = "请输入用户名")]
			[MinLength(3, ErrorMessage = "用户名过短")]
			[MaxLength(200, ErrorMessage = "用户名位数过长")]
			public string? UserName { get; set; }
		}
	}
}
