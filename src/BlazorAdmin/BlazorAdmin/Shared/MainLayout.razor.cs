namespace BlazorAdmin.Shared
{
	public partial class MainLayout
	{
		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			_themeState.IsDarkChangeEvent += StateHasChanged;
			_themeState.ThemeChangeEvent += StateHasChanged;
		}
	}
}
