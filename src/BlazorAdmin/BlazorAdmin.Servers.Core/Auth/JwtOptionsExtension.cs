using BlazorAdmin.Servers.Core.Data;
using BlazorAdmin.Servers.Core.Data.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace BlazorAdmin.Servers.Core.Auth
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
