using BlazorAdmin.Data.Constants;
using Microsoft.JSInterop;

namespace BlazorAdmin.Web.Components.Shared
{
    public partial class NoAuthorizedPage
    {
        private async Task LogoutClick()
        {
            var cookieUtil = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/cookieUtil.js");
            await cookieUtil.InvokeVoidAsync("setCookie", CommonConstant.UserToken, string.Empty);

            _navManager.NavigateTo("/login", true);
        }
    }
}
