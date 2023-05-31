using BlazorAdmin.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace BlazorAdmin.Core.Auth
{
	public class JwtAuthStateProvider : AuthenticationStateProvider
	{

		private AuthenticationState currentUser = new AuthenticationState(new ClaimsPrincipal());

		public JwtAuthStateProvider(ExternalAuthService service)
		{
			//var token = localStorage.GetAsync<string>(CommonConstant.UserToken).Result;

			//if (!string.IsNullOrEmpty(token.Value))
			//{
			//	currentUser = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
			//	{
			//		new Claim(ClaimConstant.UserName,"tenka")
			//	}, "Local")));
			//}

			service.UserChanged += (newUser) =>
			{
				currentUser = new AuthenticationState(newUser);
				NotifyAuthenticationStateChanged(Task.FromResult(currentUser));
			};
		}

		public override async Task<AuthenticationState> GetAuthenticationStateAsync()
		{

			return currentUser;
		}
	}
	public class ExternalAuthService
	{
		private ProtectedLocalStorage _localStorage;
		public event Action<ClaimsPrincipal>? UserChanged;
		private ClaimsPrincipal? currentUser;

		public ExternalAuthService(ProtectedLocalStorage localStorage)
		{
			_localStorage = localStorage;
		}

		public ClaimsPrincipal CurrentUser
		{
			get { return currentUser ?? new(); }
			set
			{
				currentUser = value;

				if (UserChanged is not null)
				{
					UserChanged(currentUser);
				}
			}
		}

		public async Task SetCurrentUser()
		{
			var token = await _localStorage.GetAsync<string>(CommonConstant.UserToken);

			if (!string.IsNullOrEmpty(token.Value))
			{
				CurrentUser = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
				{
					new Claim(ClaimConstant.UserName,"tenka")
				}, "Local"));
			}
		}
	}
}
