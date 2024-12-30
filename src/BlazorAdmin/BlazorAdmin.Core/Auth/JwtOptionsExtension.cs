using BlazorAdmin.Data.Constants;
using BlazorAdmin.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin.Core.Auth
{
    public static class JwtOptionsExtension
    {

        public static async void InitialJwtOptions(JwtBearerOptions options)
        {
            using var scope = CurrentApplication.Application.Services.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlazorAdminDbContext>>();
            using var context = await dbContextFactory.CreateDbContextAsync();

            var issuer = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtIssue)!.Value;
            var audience = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtAudience)!.Value;
            var privateKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPrivateKey)!.Value;
            var publicKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPublicKey)!.Value;

            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out int publicReadBytes);
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out int privateReadBytes);
            var securityKey = new RsaSecurityKey(rsa);
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudience = audience,
                ValidateAudience = false,

                ValidIssuer = issuer,
                ValidateIssuer = false,

                IssuerSigningKey = securityKey,

                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };
        }
    }
}
