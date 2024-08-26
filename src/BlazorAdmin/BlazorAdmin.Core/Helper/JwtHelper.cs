using BlazorAdmin.Data;
using BlazorAdmin.Data.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace BlazorAdmin.Core.Helper
{
    public class JwtHelper
    {
        private readonly IDbContextFactory<BlazorAdminDbContext> _dbContextFactory;

        private readonly TokenValidationParameters _validationParameters;

        private static int? _expireSpan;

        private readonly SigningCredentials _credentials;

        private static RsaSecurityKey? _securityKey;

        private static string? _issuer;

        private static string? _audience;

        public JwtHelper(IDbContextFactory<BlazorAdminDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;

            using var context = _dbContextFactory.CreateDbContext();

            if (string.IsNullOrEmpty(_issuer))
            {
                _issuer = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtIssue)!.Value;
            }

            if (string.IsNullOrEmpty(_audience))
            {
                _audience = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtAudience)!.Value;
            }

            if (_securityKey is null)
            {
                var privateKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPrivateKey)!.Value;
                var publicKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPublicKey)!.Value;

                // firstly public key，secondly private key
                var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out int publicReadBytes);
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out int privateReadBytes);
                _securityKey = new RsaSecurityKey(rsa);
            }


            _validationParameters = new TokenValidationParameters
            {
                ValidAudience = _audience,
                ValidateAudience = false,

                ValidIssuer = _issuer,
                ValidateIssuer = false,

                IssuerSigningKey = _securityKey,
                ValidateIssuerSigningKey = true,

                ValidateLifetime = true
            };

            if (_expireSpan == null)
            {
                var expireSpan = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtExpireMinute)!.Value;
                _expireSpan = int.Parse(expireSpan);
            }

            _credentials = new SigningCredentials(_validationParameters.IssuerSigningKey, SecurityAlgorithms.RsaSha256);
        }

        public static void ResetCache()
        {
            _securityKey = null;
            _issuer = null;
            _audience = null;
            _expireSpan = null;
        }

        public string GenerateJwtToken(List<Claim> claims)
        {
            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _validationParameters.ValidIssuer,
                audience: _validationParameters.ValidAudience,
                expires: DateTime.Now.AddMinutes(_expireSpan.Value),
                claims: claims,
                signingCredentials: _credentials);
            var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return token;
        }

        public ClaimsPrincipal? ValidToken(string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return null;
                }
                return new JwtSecurityTokenHandler().ValidateToken(token, _validationParameters,
                    out var securityToken);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public TokenValidationParameters GetValidationParameters()
        {
            return _validationParameters;
        }
    }
}
