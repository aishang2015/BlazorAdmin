using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Auth
{
    public class ApiAuthorizeHandler : AuthorizationHandler<ApiAuthorizeRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiAuthorizeRequirement requirement)
        {
            context.Succeed(requirement);
        }
    }
}
