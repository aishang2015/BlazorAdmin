using Microsoft.AspNetCore.Authorization;

namespace BlazorAdmin.Servers.Core.Auth
{
    public class ApiAuthorizeHandler : AuthorizationHandler<ApiAuthorizeRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiAuthorizeRequirement requirement)
        {
            context.Succeed(requirement);
        }
    }
}
