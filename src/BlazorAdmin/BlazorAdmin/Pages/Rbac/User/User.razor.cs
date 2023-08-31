using BlazorAdmin.Pages.Rbac.User.Dialogs;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

namespace BlazorAdmin.Pages.Rbac.User
{
	public partial class User
	{
		private List<UserModel> Users = new List<UserModel>();

		private int Page = 1;

		private int Size = 10;

		private int Total = 0;

		private string? SearchText;

		protected override async Task OnInitializedAsync()
		{
			await InitialData();
		}

		private async Task InitialData()
		{
			using var context = await _dbFactory.CreateDbContextAsync();

			IQueryable<Data.Entities.User> query = context.Users.Where(u => !u.IsDeleted);
			if (!string.IsNullOrEmpty(SearchText))
			{
				query = query.Where(u => u.Name.Contains(SearchText));
			}

			Users = await query.Skip((Page - 1) * Size).Take(Size)
				.Select(p => new UserModel
				{
					Id = p.Id,
					Avatar = p.Avatar,
					Name = p.Name,
					RealName = p.RealName,
					IsEnabled = p.IsEnabled
				}).ToListAsync();
			Total = await query.CountAsync();

			foreach (var user in Users)
			{
				user.Number = (Page - 1) * Size + Users.IndexOf(user) + 1;
			}
		}

		private async void PageChangedClick(int page)
		{
			Page = page;
			await InitialData();
		}

		private async Task AddUserClick()
		{
			var parameters = new DialogParameters { };
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<CreateUserDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialData();
			}
		}

		private async Task DeleteUserClick(int userId)
		{
			var parameters = new DialogParameters
			{
				{"UserId",userId }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<DeleteUserDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialData();
			}
		}


		private async Task EditUserClick(int userId)
		{
			var parameters = new DialogParameters
			{
				{"UserId",userId }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<UpdateUserDialog>(string.Empty, parameters, options).Result;
			if (!result.Canceled)
			{
				await InitialData();
			}
		}

		private async Task ChangePasswordClick(int userId)
		{
			var parameters = new DialogParameters
			{
				{"UserId",userId }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			await _dialogService.Show<ChangePasswordDialog>(string.Empty, parameters, options).Result;
		}

		private async Task SetUserRoleClick(int userId)
		{
			var parameters = new DialogParameters
			{
				{"UserId",userId }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			await _dialogService.Show<UserRoleDialog>(string.Empty, parameters, options).Result;
		}

		private async Task ChangeUserActive(int userId, bool isEnabled)
		{
			using var context = await _dbFactory.CreateDbContextAsync();
			var user = context.Users.Find(userId);
			if (user != null)
			{
				user.IsEnabled = isEnabled;
				await context.CustomSaveChangesAsync(_stateProvider);
				_snackbarService.Add("状态修改成功！", Severity.Success);
				Users.FirstOrDefault(u => u.Id == userId)!.IsEnabled = isEnabled;
			}
		}

		private class UserModel
		{
			public int Id { get; set; }

			public int Number { get; set; }

			public string? Avatar { get; set; }

			public string Name { get; set; } = null!;

			public string? RealName { get; set; }

			public bool IsEnabled { get; set; }
		}
	}
}
