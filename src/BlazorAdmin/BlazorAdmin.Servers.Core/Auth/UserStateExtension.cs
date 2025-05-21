using BlazorAdmin.Servers.Core.Extension;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorAdmin.Servers.Core.Auth
{
    public static class UserStateExtension
    {
        public static async Task<int> GetUserIdAsync(this AuthenticationStateProvider authenticationStateProvider)
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            return state.User.GetUserId();
        }

        public static async Task<string> GetUserNameAsync(this AuthenticationStateProvider authenticationStateProvider)
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            return state.User.GetUserName();
        }
    }
}
