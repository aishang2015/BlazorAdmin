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

		private readonly int _expireSpan;

		private readonly SigningCredentials _credentials;

		public JwtHelper(IDbContextFactory<BlazorAdminDbContext> dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;

			using var context = _dbContextFactory.CreateDbContext();
			var issuer = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtIssue)!.Value;
			var audience = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtAudience)!.Value;
			var privateKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPrivateKey)!.Value;
			var publicKey = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPublicKey)!.Value;

			// firstly public key，secondly private key
			var rsa = RSA.Create();
			rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out int publicReadBytes);
			rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out int privateReadBytes);
			var securityKey = new RsaSecurityKey(rsa);

			_validationParameters = new TokenValidationParameters
			{
				ValidAudience = audience,
				ValidateAudience = false,

				ValidIssuer = issuer,
				ValidateIssuer = false,

				IssuerSigningKey = securityKey,
				ValidateIssuerSigningKey = true,

				ValidateLifetime = true
			};

			var expireSpan = context.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtExpireMinute)!.Value;
			_expireSpan = int.Parse(expireSpan);

			_credentials = new SigningCredentials(_validationParameters.IssuerSigningKey, SecurityAlgorithms.RsaSha256);
		}

		public string GenerateJwtToken(List<Claim> claims)
		{
			var jwtSecurityToken = new JwtSecurityToken(
				issuer: _validationParameters.ValidIssuer,
				audience: _validationParameters.ValidAudience,
				expires: DateTime.Now.AddMinutes(_expireSpan),
				claims: claims,
				signingCredentials: _credentials);
			var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
			return token;
		}

		public ClaimsPrincipal ValidToken(string token)
		{
			try
			{
				return new JwtSecurityTokenHandler().ValidateToken(token, _validationParameters,
					out var securityToken);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
