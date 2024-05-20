using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;

namespace BlazorAdmin.Layout.States
{
    public class ThemeState
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly NavigationManager _navigationManager;

        private readonly IJSRuntime _jsRuntime;

        public ThemeState(IHttpContextAccessor httpContextAccessor,
            NavigationManager navigationManager,
            IJSRuntime jSRuntime)
        {
            _httpContextAccessor = httpContextAccessor;
            _navigationManager = navigationManager;
            _jsRuntime = jSRuntime;

            var value = _httpContextAccessor.HttpContext?.Request.Cookies.Where(c => c.Key == "IsDark")
                .FirstOrDefault().Value;
            _isDark = string.IsNullOrEmpty(value) ? false : bool.Parse(value);

            var primaryColor = _httpContextAccessor.HttpContext?.Request.Cookies.Where(c => c.Key == "PrimaryColor")
                .FirstOrDefault().Value;

            _theme = new MudTheme()
            {
                PaletteDark = new PaletteDark()
                {
                    Primary = "#007fff",
                    Tertiary = "#594AE2",
                    Black = "#27272f",
                    Background = "#121212",                 // 整体背景色
                    BackgroundGray = "#202020",
                    Surface = "#1f1f1f",                    // 表格等控件背景色
                    DrawerBackground = "#181818",
                    DrawerText = "rgba(255,255,255, 0.50)",
                    DrawerIcon = "rgba(255,255,255, 0.50)",
                    AppbarBackground = "rgb(24,24,24)",
                    AppbarText = "rgba(255,255,255, 0.70)",
                    TextPrimary = "rgba(255,255,255, 0.70)",
                    TextSecondary = "rgba(255,255,255, 0.50)",
                    ActionDefault = "rgb(173, 173, 177)",
                    //ActionDisabled = "rgba(0, 127, 255, 0.40)",
                    //ActionDisabledBackground = "rgba(0, 127, 255, 0.26)",
                    //Divider = "rgba(0, 127, 255, 0.12)",
                    //DividerLight = "rgba(20, 127, 255, 0.06)",
                    TableLines = "rgba(255, 255, 255, 0.12)",
                    //LinesDefault = "rgba(0, 127, 255, 0.12)",
                    //LinesInputs = "rgba(0, 127, 255, 0.3)",
                    TextDisabled = "rgba(0, 127, 255, 0.2)"
                },
                LayoutProperties = new LayoutProperties()
                {
                    DefaultBorderRadius = "4px",
                }
            };

            if (string.IsNullOrEmpty(primaryColor))
            {
                primaryColor = "#1668dc";
            }

            var color = new MudColor(primaryColor);
            UpdatePaletteColor(color);
        }

        private void UpdatePaletteColor(MudColor color)
        {
            _theme.PaletteLight.Primary = color;
            _theme.PaletteLight.PrimaryDarken = color.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
            _theme.PaletteLight.PrimaryLighten = color.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
            _theme.PaletteLight.AppbarBackground = color;

            _theme.PaletteDark.Primary = color;
            _theme.PaletteDark.PrimaryDarken = color.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
            _theme.PaletteDark.PrimaryLighten = color.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
        }

        private bool _isDark;

        private MudTheme _theme = new();

        public event Action? ThemeChangeEvent;

        public event Action? IsDarkChangeEvent;

        public void LoadTheme()
        {
            IsDarkStateChanged();
            ThemeStateChanged();
        }

        public bool IsDark
        {
            get
            {
                return _isDark;
            }
            set
            {
                _isDark = value;
                SetCookie("IsDark", value.ToString());
            }
        }

        public MudColor PrimaryColor
        {
            get
            {
                return _theme.PaletteLight.Primary;
            }
            set
            {
                UpdatePaletteColor(value);
                SetCookie("PrimaryColor", value.Value);
            }
        }

        private async Task SetCookie(string key, string value)
        {
            var cookieUtil = await _jsRuntime.InvokeAsync<IJSObjectReference>
                    ("import", "./js/cookieUtil.js");
            await cookieUtil.InvokeVoidAsync("setCookie", key, value);
            IsDarkStateChanged();
            ThemeStateChanged();
        }

        public MudTheme MudTheme
        {
            get
            {
                return _theme;
            }
        }

        private void ThemeStateChanged() => ThemeChangeEvent?.Invoke();
        private void IsDarkStateChanged() => IsDarkChangeEvent?.Invoke();
    }
}
