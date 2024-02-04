using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using MudBlazor.Utilities;
using System.Drawing;
using static MudBlazor.Colors;

namespace BlazorAdmin.Web.Data.States
{
	public class ThemeState
	{
		private ProtectedLocalStorage _protectedLocalStorage;

		public ThemeState(ProtectedLocalStorage protectedLocalStorage)
		{
			_protectedLocalStorage = protectedLocalStorage;
		}

		private bool _isDark;

		private MudTheme _theme = new();

		public event Action? ThemeChangeEvent;

		public event Action? IsDarkChangeEvent;

		public async Task LoadTheme()
		{
			_theme = new MudTheme()
			{
				PaletteDark = new PaletteDark()
				{
					Primary = "#007fff",
					Tertiary = "#594AE2",
					Black = "#27272f",
					Background = "#121212",                 // 整体背景色
					BackgroundGrey = "#202020",
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

			try
			{
				var localData = await _protectedLocalStorage.GetAsync<bool>("IsDark");
				if (localData.Success)
				{
					_isDark = localData.Value;
				}

				var primaryColor = await _protectedLocalStorage.GetAsync<string>("PrimaryColor");
				if (primaryColor.Success)
				{
					var color = new MudColor(primaryColor.Value);
					_theme.Palette.Primary = color;
					_theme.Palette.PrimaryDarken = color.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
					_theme.Palette.PrimaryLighten = color.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
					_theme.Palette.AppbarBackground = color;

					_theme.PaletteDark.Primary = color;
					_theme.PaletteDark.PrimaryDarken = color.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
					_theme.PaletteDark.PrimaryLighten = color.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
				}
				IsDarkStateChanged();
				ThemeStateChanged();
			}
			catch (Exception)
			{
				IsDark = true;
			}
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
				_protectedLocalStorage.SetAsync("IsDark", value);
				IsDarkStateChanged();
			}
		}

		public MudTheme MudTheme
		{
			get
			{
				return _theme;
			}
		}

		public MudColor PrimaryColor
		{
			get
			{
				return _theme.Palette.Primary;
			}
			set
			{
				_protectedLocalStorage.SetAsync("PrimaryColor", value.Value);

				_theme.Palette.Primary = value;
				_theme.Palette.PrimaryDarken = value.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
				_theme.Palette.PrimaryLighten = value.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
				_theme.Palette.AppbarBackground = value;

				_theme.PaletteDark.Primary = value;
				_theme.PaletteDark.PrimaryDarken = value.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
				_theme.PaletteDark.PrimaryLighten = value.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);

				ThemeStateChanged();
			}
		}


		private void ThemeStateChanged() => ThemeChangeEvent?.Invoke();
		private void IsDarkStateChanged() => IsDarkChangeEvent?.Invoke();
	}
}
