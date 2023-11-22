using BlazorAdmin.Core.Constants;
using BlazorAdmin.Core.Helper;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;

namespace BlazorAdmin.Core.Auth
{
	public class JwtAuthStateProvider : AuthenticationStateProvider
	{

		private AuthenticationState currentUser = new AuthenticationState(new ClaimsPrincipal());

		public JwtAuthStateProvider(ExternalAuthService service)
		{
			service.UserChanged += (newUser) =>
			{
				currentUser = new AuthenticationState(newUser);
				NotifyAuthenticationStateChanged(Task.FromResult(currentUser));
			};
		}

		public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(currentUser);
	}

	public class ExternalAuthService
	{
		private ProtectedLocalStorage _localStorage;
		public event Action<ClaimsPrincipal>? UserChanged;
		private ClaimsPrincipal? currentUser;

		private JwtHelper _jwtHelper;

		public ExternalAuthService(ProtectedLocalStorage localStorage,
			JwtHelper jwtHelper)
		{
			_localStorage = localStorage;
			_jwtHelper = jwtHelper;
		}

		public async Task SetCurrentUser()
		{
			ProtectedBrowserStorageResult<string> token;
			try
			{
				token = await _localStorage.GetAsync<string>(CommonConstant.UserToken);
			}
			catch (System.Security.Cryptography.CryptographicException)
			{
				if (UserChanged is not null)
				{
					UserChanged(new ClaimsPrincipal());
				}
				return;
			}

			if (!string.IsNullOrEmpty(token.Value))
			{
				var user = _jwtHelper.ValidToken(token.Value);
				if (user == null)
				{
					user = new ClaimsPrincipal();
				}
				if (UserChanged is not null)
				{
					UserChanged(user);
				}
			}
		}
	}
}
