namespace BlazorAdmin.Web.Components.Layout
{
    public partial class EmptyLayout
    {
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                _themeState.IsDarkChangeEvent += StateHasChanged;
                _themeState.ThemeChangeEvent += StateHasChanged;
                _themeState.LoadTheme();
            }
        }
    }
}
