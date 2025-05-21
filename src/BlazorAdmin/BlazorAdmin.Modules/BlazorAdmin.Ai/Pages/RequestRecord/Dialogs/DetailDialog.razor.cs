using BlazorAdmin.Data.Entities.Ai;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Ai.Pages.RequestRecord.Dialogs
{
    public partial class DetailDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; } = null!;

        [Parameter] public int RecordId { get; set; }

        private AiRequestRecord? Record { get; set; }

        protected override async Task OnInitializedAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            Record = await context.AiRequestRecords.FindAsync(RecordId);
        }

        private void Cancel()
        {
            MudDialog.Close();
        }
    }
}