using BlazorAdmin.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BlazorAdmin.Shared
{
	public partial class Login
	{
		private Dictionary<string, object> InputAttributes { get; set; } =
			new Dictionary<string, object>()
				{
				   { "autocomplete", "off2" },
				};

		private bool ShowContent;

		private LoginModel _loginModel = new();

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				await _authService.SetCurrentUser();
				var state = await _stateProvider.GetAuthenticationStateAsync();
				if (state.User.Identity != null && state.User.Identity.IsAuthenticated)
				{
					_navManager.NavigateTo("/");
				}
				ShowContent = true;
				StateHasChanged();
			}
		}

		private async Task LoginSubmit()
		{
			await _localStorage.SetAsync(CommonConstant.UserToken, "token");
			await _authService.SetCurrentUser();
			_navManager.NavigateTo("/");
		}

		private record LoginModel
		{
			[Required(ErrorMessage = "请输入用户名")]
			public string? UserName { get; set; }

			[Required(ErrorMessage = "请输入密码")]
			public string? Password { get; set; }
		}
	}
}
