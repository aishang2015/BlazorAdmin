﻿using BlazorAdmin.Data.Constants;
using System.Security.Claims;

namespace BlazorAdmin.Core.Extension
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimConstant.UserId)!.Value;
            return int.Parse(userId);
        }

        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.Claims.FirstOrDefault(c => c.Type == ClaimConstant.UserName)!.Value;
        }
    }
}
