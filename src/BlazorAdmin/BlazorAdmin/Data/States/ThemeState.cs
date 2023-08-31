using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;
using MudBlazor.Utilities;

namespace BlazorAdmin.Data.States
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
			try
			{
				var localData = await _protectedLocalStorage.GetAsync<bool>("IsDark");
				if (localData.Success)
				{
					IsDark = localData.Value;
				}

				var primaryColor = await _protectedLocalStorage.GetAsync<string>("PrimaryColor");
				if (primaryColor.Success)
				{
					var color = new MudColor(primaryColor.Value);
					_theme.Palette.Primary = color;
					_theme.Palette.AppbarBackground = color;
					_theme.PaletteDark.Primary = color;

					_theme.Palette.PrimaryDarken = color.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
					_theme.Palette.PrimaryLighten = color.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
					_theme.PaletteDark.PrimaryDarken = color.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
					_theme.PaletteDark.PrimaryLighten = color.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
				}
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
				_theme.Palette.AppbarBackground = value;
				_theme.PaletteDark.Primary = value;

				_theme.Palette.PrimaryDarken = value.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
				_theme.Palette.PrimaryLighten = value.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
				_theme.PaletteDark.PrimaryDarken = value.ColorRgbDarken().ToString(MudColorOutputFormats.RGB);
				_theme.PaletteDark.PrimaryLighten = value.ColorRgbLighten().ToString(MudColorOutputFormats.RGB);
				ThemeStateChanged();
			}
		}


		private void ThemeStateChanged() => ThemeChangeEvent?.Invoke();
		private void IsDarkStateChanged() => IsDarkChangeEvent?.Invoke();
	}
}
