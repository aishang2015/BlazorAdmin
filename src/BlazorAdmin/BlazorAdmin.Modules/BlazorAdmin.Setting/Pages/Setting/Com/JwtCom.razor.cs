using BlazorAdmin.Core.Constants;
using BlazorAdmin.Core.Data;
using BlazorAdmin.Setting.Pages.Setting.Dialogs;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Setting.Pages.Setting.Com
{
	public partial class JwtCom
	{
		private JwtConfig JwtConfigModel;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			await GetSettingsAsync();
		}

		private Task GetSettingsAsync()
		{
			using var dbContext = _dbFactory.CreateDbContext();
			JwtConfigModel = new JwtConfig
			{
				Issuer = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtIssue)!.Value,
				Audience = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtAudience)!.Value,
				ExpireMinutes = int.Parse(dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtExpireMinute)!.Value),
				RsaPrivateKey = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPrivateKey)!.Value,
				RsaPublicKey = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPublicKey)!.Value,
			};
			return Task.CompletedTask;
		}

		private async Task GenerateNewRsa()
		{
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
			var result = await _dialogService.Show<ConfirmUpdateRsa>(string.Empty, new DialogParameters(), options).Result;
			if (!result.Canceled)
			{
				var rsa = RSA.Create();
				JwtConfigModel.RsaPrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
				JwtConfigModel.RsaPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
			}
		}

		private async Task JwtConfigSubmit()
		{
			using var dbContext = _dbFactory.CreateDbContext();
			var issuer = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtIssue);
			issuer.Value = JwtConfigModel.Issuer;
			var audience = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtAudience);
			audience.Value = JwtConfigModel.Audience;
			var expireMinutes = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtExpireMinute);
			expireMinutes.Value = JwtConfigModel.ExpireMinutes.ToString();
			var rsaPrivateKey = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPrivateKey);
			rsaPrivateKey.Value = JwtConfigModel.RsaPrivateKey;
			var rsaPublicKey = dbContext.Settings.FirstOrDefault(s => s.Key == JwtConstant.JwtSigningRsaPublicKey);
			rsaPublicKey.Value = JwtConfigModel.RsaPublicKey;
			await dbContext.CustomSaveChangesAsync(_stateProvider);

			_snackbarService.Add("保存成功！", Severity.Success);
		}

		private class JwtConfig
		{
			public required string Issuer { get; set; }

			public required string Audience { get; set; }

			public required string RsaPrivateKey { get; set; }

			public required string RsaPublicKey { get; set; }

			public int ExpireMinutes { get; set; }
		}
	}
}
