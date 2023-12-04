
using BlazorAdmin.Core.Constants;
using BlazorAdmin.Web.Components.Shared.Dialogs.Layout;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace BlazorAdmin.Web.Components.Layout
{
	public partial class AuthorizedLayout
	{
		[Parameter] public RenderFragment? Child { get; set; }

		bool _drawerOpen = true;

		bool visible;

		DialogOptions dialogOptions = new() { NoHeader = true };

		string _userName = string.Empty;

		string? _avatar = string.Empty;

		CultureInfo _culture;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			_culture = CultureInfo.CurrentCulture;

			var currentUri = _navManager.Uri.Substring(_navManager.BaseUri.Length - 1);
			if (currentUri.Contains('?'))
			{
				currentUri = currentUri.Substring(0, currentUri.IndexOf('?'));
			}

			var couldAccess = await _accessService.CheckUriCanAccess(currentUri);
			if (!couldAccess)
			{
				_navManager.NavigateTo("/noauthorized");
			}
			else
			{
				var user = await _stateProvider.GetAuthenticationStateAsync();
				using var context = await _dbFactory.CreateDbContextAsync();
				var userInfo = context.Users.Find(user.User.GetUserId());
				_userName = userInfo.RealName;
				_avatar = userInfo.Avatar;
			}

		}

		void DrawerToggle()
		{
			_drawerOpen = !_drawerOpen;
		}

		private void CultureChanged(string culture)
		{
			if (!string.IsNullOrEmpty(culture))
			{
				var uri = new Uri(_navManager.Uri)
						.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
				var cultureEscaped = Uri.EscapeDataString(culture);
				var uriEscaped = Uri.EscapeDataString(uri);

				_navManager.NavigateTo(
					$"api/Culture/Set?culture={cultureEscaped}&redirectUri={uriEscaped}",
					forceLoad: true);
			}
		}

		private async Task ShowUserSettings()
		{
			var parameters = new DialogParameters { };
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			await _dialogService.Show<ProfileSetting>(string.Empty, parameters, options).Result;

			using var context = await _dbFactory.CreateDbContextAsync();
			var user = await _stateProvider.GetAuthenticationStateAsync();
			var userInfo = context.Users.Find(user.User.GetUserId());
			_userName = userInfo.RealName;
			_avatar = userInfo.Avatar;
			StateHasChanged();
		}


		private async Task LogoutClick()
		{
			await _localStorage.DeleteAsync(CommonConstant.UserToken);
			await _authService.SetCurrentUser();
			_navManager.NavigateTo("/login", true);
		}
	}
}
