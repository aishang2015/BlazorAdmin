using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities.Ai;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using BlazorAdmin.Ai.Resources;

namespace BlazorAdmin.Ai.Pages.Config.Dialogs
{
    public partial class EditConfigDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter] public int ConfigId { get; set; }

        private AiConfig Config { get; set; } = new();

        private MudForm form = null!;

        protected override async Task OnInitializedAsync()
        {
            if (ConfigId != 0)
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var config = await context.AiConfigs.FindAsync(ConfigId);
                if (config != null)
                {
                    Config = new AiConfig
                    {
                        Id = config.Id,
                        ConfigName = config.ConfigName,
                        Endpoint = config.Endpoint,
                        ApiKey = config.ApiKey,
                        ModelName = config.ModelName,
                        ContextLength = config.ContextLength,
                        InputPricePerToken = config.InputPricePerToken,
                        OutputPricePerToken = config.OutputPricePerToken,
                        Description = config.Description,
                    };
                }
            }
        }

        private async Task Submit()
        {
            await form.Validate();
            if (form.IsValid)
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                if (ConfigId == 0)
                {
                    context.AiConfigs.Add(Config);
                    await context.SaveChangesAsync();
                    _snackbarService.Add(Loc["AIConfig_CreateSuccess"], Severity.Success);
                }
                else
                {
                    context.Entry(Config).State = EntityState.Modified;
                    await context.SaveChangesAsync();
                    _snackbarService.Add(Loc["AIConfig_EditSuccess"], Severity.Success);
                }
                MudDialog.Close(DialogResult.Ok(true));
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}