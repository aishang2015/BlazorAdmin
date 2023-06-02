using BlazorAdmin.Data.Entities;
using BlazorAdmin.Pages.Dialogs.Role;
using BlazorAdmin.Pages.Dialogs.User;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BlazorAdmin.Pages.Settings
{
	public partial class Role
	{
		private int Page = 1;

		private int Size = 10;

		private int Total = 0;

		private string? SearchText;

		private List<RoleModel> Roles = new();

		protected override async Task OnInitializedAsync()
		{
			await InitialData();
		}

		private async Task InitialData()
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			IQueryable<Data.Entities.Role> query = context.Roles.AsQueryable();
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

		private async Task ChangeRoleActive(int roleId, bool isEnabled)
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var role = context.Roles.Find(roleId);
			if (role != null)
			{
				role.IsEnabled = isEnabled;
				await context.SaveChangesAsync();
				_snackbarService.Add("状态修改成功！", Severity.Success);
				Roles.FirstOrDefault(u => u.Id == roleId)!.IsEnabled = isEnabled;
			}
		}

		private async Task AddRoleClick()
		{
			var parameters = new DialogParameters { };
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<CreateRoleDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialData();
			}
		}

		private async Task DeleteRoleClick(int roleId)
		{
			var parameters = new DialogParameters
			{
				{"RoleId",roleId }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<DeleteRoleDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialData();
			}
		}

		private async Task EditRoleClick(int roleId)
		{
			var parameters = new DialogParameters
			{
				{"RoleId",roleId }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<UpdateRoleDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialData();
			}
		}

		private async void PageChangedClick(int page)
		{
			Page = page;
			await InitialData();
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
