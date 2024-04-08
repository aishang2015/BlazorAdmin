using BlazorAdmin.Data.Constants;
using BlazorAdmin.Data.Extensions;
using BlazorAdmin.Setting.Pages.Setting.Dialogs;
using MudBlazor;
using System.Security.Cryptography;

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
                Issuer = dbContext.Settings.GetSettingValue(JwtConstant.JwtIssue),
                Audience = dbContext.Settings.GetSettingValue(JwtConstant.JwtAudience),
                ExpireMinutes = int.Parse(dbContext.Settings.GetSettingValue(JwtConstant.JwtExpireMinute)!),
                RsaPrivateKey = dbContext.Settings.GetSettingValue(JwtConstant.JwtSigningRsaPrivateKey),
                RsaPublicKey = dbContext.Settings.GetSettingValue(JwtConstant.JwtSigningRsaPublicKey),
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
            dbContext.Settings.SetSettingValue(JwtConstant.JwtIssue, JwtConfigModel.Issuer);
            dbContext.Settings.SetSettingValue(JwtConstant.JwtAudience, JwtConfigModel.Audience);
            dbContext.Settings.SetSettingValue(JwtConstant.JwtExpireMinute, JwtConfigModel.ExpireMinutes.ToString());
            dbContext.Settings.SetSettingValue(JwtConstant.JwtSigningRsaPrivateKey, JwtConfigModel.RsaPrivateKey);
            dbContext.Settings.SetSettingValue(JwtConstant.JwtSigningRsaPublicKey, JwtConfigModel.RsaPublicKey);
            await dbContext.AuditSaveChangesAsync();

            _snackbarService.Add("保存成功！", Severity.Success);
        }

        private class JwtConfig
        {
            public string? Issuer { get; set; }

            public string? Audience { get; set; }

            public string? RsaPrivateKey { get; set; }

            public string? RsaPublicKey { get; set; }

            public int ExpireMinutes { get; set; }
        }
    }
}
