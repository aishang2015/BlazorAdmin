using Microsoft.AspNetCore.Authorization;

namespace BlazorAdmin.Core.Auth
{
    public class ApiAuthorizeAttribute : AuthorizeAttribute
    {
        public ApiAuthorizeAttribute()
        {
            // 可以在这里设置默认的策略或其他属性
            Policy = "ApiAuthorizePolicy";
        }
    }
}
