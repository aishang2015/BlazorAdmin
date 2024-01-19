using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MudBlazor;

namespace BlazorAdmin.Log.Pages.AuditLog.Dialogs
{
    public partial class AuditLogDetailDialog
    {
        [Parameter] public Guid AuditLogId { get; set; }

        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        private List<AuditLogDetailModel> AuditLogDetails { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitialAsync();
        }

        private async Task InitialAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            AuditLogDetails = context.AuditLogDetails.Where(d => d.AuditLogId == AuditLogId)
                .Select(d => new AuditLogDetailModel
                {
                    Id = d.Id,
                    EntityName = d.EntityName,
                    PropertyName = d.PropertyName,
                    OldValue = d.OldValue,
                    NewValue = d.NewValue,
                }).ToList();

            if (AuditLogDetails.Count > 0)
            {
                var model = context.GetService<IDesignTimeModel>().Model
                    .FindEntityType(AuditLogDetails.First().EntityName);
                AuditLogDetails.ForEach(d =>
                {
                    d.Number = AuditLogDetails.IndexOf(d) + 1;
                    d.EntityName = model!.GetComment()!;
                    d.PropertyName = model!.FindDeclaredProperty(d.PropertyName)!.GetComment()!;
                });
            }
        }

        private class AuditLogDetailModel
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string EntityName { get; set; } = null!;
            public string PropertyName { get; set; } = null!;
            public string? OldValue { get; set; }
            public string? NewValue { get; set; }
        }
    }
}
