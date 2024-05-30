using BlazorAdmin.Core.Extension;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Auth
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
