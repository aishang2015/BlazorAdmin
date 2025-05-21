using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace BlazorAdmin.Servers.Core.Auth
{

    // https://github.com/dotnet/aspnetcore/issues/52063
    // AuthorizeRouteView 不起作用
    public class BlazorAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            // 检查是否是自定义的授权策略
            if (policy.Requirements.OfType<ApiAuthorizeRequirement>().Any() &&
                !authorizeResult.Succeeded)
            {
                // 返回401
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            return next(context);
        }
    }
}
