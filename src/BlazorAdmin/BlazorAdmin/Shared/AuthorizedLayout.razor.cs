using BlazorAdmin.Constants;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;
using System.Text.RegularExpressions;
using static MudBlazor.Colors;

namespace BlazorAdmin.Shared
{
	public partial class AuthorizedLayout
	{
		[Parameter] public RenderFragment? Child { get; set; }

		bool _drawerOpen = true;

		string _userName = string.Empty;

		CultureInfo _culture ;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			_culture = CultureInfo.CurrentCulture;

			var user = await _stateProvider.GetAuthenticationStateAsync();
			_userName = user.User.GetUserName();

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

		private async Task LogoutClick()
		{
			await _localStorage.DeleteAsync(CommonConstant.UserToken);
			await _authService.SetCurrentUser();
			_navManager.NavigateTo("/login", true);
		}
	}
}
