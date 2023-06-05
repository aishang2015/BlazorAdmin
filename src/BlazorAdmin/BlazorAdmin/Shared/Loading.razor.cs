namespace BlazorAdmin.Shared
{
	public partial class Loading
	{
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				await _themeState.LoadTheme();
				await _authService.SetCurrentUser();
				var state = await _stateProvider.GetAuthenticationStateAsync();
				if (state.User.Identity == null || !state.User.Identity.IsAuthenticated)
				{
					_navManager.NavigateTo("/login");
				}

			}
		}
	}
}
