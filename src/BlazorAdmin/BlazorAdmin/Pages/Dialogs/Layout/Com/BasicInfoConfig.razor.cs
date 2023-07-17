using FluentCodeServer.Core;
using MudBlazor;

namespace BlazorAdmin.Pages.Dialogs.Layout.Com
{
	public partial class BasicInfoConfig
	{
		private Data.Entities.User? _user = new();

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			using var context = await _dbFactory.CreateDbContextAsync();
			var user = await _stateProvider.GetAuthenticationStateAsync();
			_user = context.Users.Find(user.User.GetUserId());
		}

		private async Task ChangePwd()
		{
			var parameters = new DialogParameters { };
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			await _dialogService.Show<ChangePasswordDialog>(string.Empty, parameters, options).Result;
		}

	}
}
