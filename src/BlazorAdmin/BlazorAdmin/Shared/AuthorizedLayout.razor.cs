using BlazorAdmin.Constants;
using Microsoft.AspNetCore.Components;

namespace BlazorAdmin.Shared
{
	public partial class AuthorizedLayout
	{
		[Parameter] public RenderFragment? Child { get; set; }

		bool _drawerOpen = true;

		void DrawerToggle()
		{
			_drawerOpen = !_drawerOpen;
		}

		private async Task LogoutClick()
		{
			await _localStorage.DeleteAsync(CommonConstant.UserToken);
			await _authService.SetCurrentUser();
			_navManager.NavigateTo("/login", true);
		}
	}
}
