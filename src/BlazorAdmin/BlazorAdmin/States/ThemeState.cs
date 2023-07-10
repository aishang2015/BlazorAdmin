using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;

namespace BlazorAdmin.States
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
			set
			{
				_theme = value;
				ThemeStateChanged();
			}
		}


		private void ThemeStateChanged() => ThemeChangeEvent?.Invoke();
		private void IsDarkStateChanged() => IsDarkChangeEvent?.Invoke();
	}
}
