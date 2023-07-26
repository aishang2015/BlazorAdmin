namespace BlazorAdmin.Shared
{
	public partial class EmptyLayout
	{
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				_themeState.IsDarkChangeEvent += StateHasChanged;
				_themeState.ThemeChangeEvent += StateHasChanged;
				await _themeState.LoadTheme();
			}
		}
	}
}
