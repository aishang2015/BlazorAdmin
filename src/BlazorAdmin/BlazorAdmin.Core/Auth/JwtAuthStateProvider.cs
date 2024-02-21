using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data.Constants;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BlazorAdmin.Core.Auth
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;

        private readonly JwtHelper _jwtHelper;

        private AuthenticationState currentUser = new AuthenticationState(new ClaimsPrincipal());

        public JwtAuthStateProvider(ExternalAuthService service, IHttpContextAccessor httpContextAccessor,
             JwtHelper jwtHelper)
        {
            _contextAccessor = httpContextAccessor;
            _jwtHelper = jwtHelper;
            service.UserChanged += (newUser) =>
            {
                currentUser = new AuthenticationState(newUser);
                NotifyAuthenticationStateChanged(Task.FromResult(currentUser));
            };

            var tokenCookie = _contextAccessor.HttpContext?.Request.Cookies
                .FirstOrDefault(c => c.Key == CommonConstant.UserToken).Value;
            if (!string.IsNullOrEmpty(tokenCookie))
            {
                var user = _jwtHelper.ValidToken(tokenCookie);
                currentUser = new AuthenticationState(user);
            }
            else
            {
                currentUser = new AuthenticationState(new ClaimsPrincipal());
            }
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(currentUser);
    }

    public class ExternalAuthService
    {
        public event Action<ClaimsPrincipal>? UserChanged;

        private JwtHelper _jwtHelper;

        public ExternalAuthService(JwtHelper jwtHelper)
        {
            _jwtHelper = jwtHelper;
        }

        public void SetCurrentUser(string token)
        {
            var user = _jwtHelper.ValidToken(token);
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
