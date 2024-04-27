using BlazorAdmin.Component.Pages;
using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data.Attributes;
using BlazorAdmin.Log.Pages.AuditLog.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MudBlazor;
using System.Reflection;

namespace BlazorAdmin.Log.Pages.AuditLog
{
    public partial class AuditLog
    {

        private PageDataGrid<AuditLogModel> dataGrid = null!;

        private List<AuditLogModel> AuditLogs = new();

        private List<Operator> Operators = new();

        private List<OperateTarget> OperateTargets = new();

        private string? InputTransaction;

        private string? SelectedUser;

        private string? SelectedOperateTarget;

        private string? SelectedOperation;

        private int Page = 1;

        private int Size = 10;

        private int Total = 0;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await InitialAsync();
        }

        private async Task InitialAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var auditLogQuery = context.AuditLogs
                .AndIf(!string.IsNullOrEmpty(InputTransaction) && Guid.TryParse(InputTransaction, out Guid inputGuid),
                        q => q.TransactionId == Guid.Parse(InputTransaction!))
                .AndIfExist(SelectedUser, q => q.UserId == int.Parse(SelectedUser!))
                .AndIfExist(SelectedOperateTarget, q => q.EntityName == SelectedOperateTarget)
                .AndIfExist(SelectedOperation, q => q.Operation == int.Parse(SelectedOperation!));

            var query = from log in auditLogQuery
                        join user in context.Users on log.UserId equals user.Id
                        orderby log.OperateTime descending
                        select new
                        {
                            log.Id,
                            log.TransactionId,
                            user.RealName,
                            log.Operation,
                            log.OperateTime,
                            log.EntityName,
                        };
            var model = context.GetService<IDesignTimeModel>().Model;
            AuditLogs = query.Skip((Page - 1) * Size).Take(Size).ToList()
                .Select((d, i) => new AuditLogModel
                {
                    Number = i + (Page - 1) * Size + 1,
                    Id = d.Id,
                    TransactionId = d.TransactionId,
                    EntityName = model.FindEntityType(d.EntityName)?.GetComment(),
                    OperateTime = d.OperateTime,
                    Operation = d.Operation,
                    UserName = d.RealName
                }).ToList();
            Total = query.Count();

            Operators = context.Users.Where(u => !u.IsSpecial).Select(u => new Operator
            {
                Id = u.Id,
                UserName = u.RealName,
            }).ToList();

            OperateTargets = model.GetEntityTypes()
                .Where(t => t.ClrType.GetCustomAttribute<IgnoreAuditAttribute>() == null)
                .Select(t => new OperateTarget
                {
                    DisplayName = t.GetComment() ?? "",
                    EntityName = t.Name
                }).ToList();
        }

        private async Task<GridData<AuditLogModel>> GetTableData(GridState<AuditLogModel> gridState)
        {
            await InitialAsync();
            return new GridData<AuditLogModel>() { TotalItems = AuditLogs.Count, Items = AuditLogs };
        }

        private async Task ViewDetail(Guid id)
        {
            var parameters = new DialogParameters
            {
                {"AuditLogId",id }
            };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            await _dialogService.Show<AuditLogDetailDialog>(string.Empty, parameters, options).Result;
        }

        private void PageChangedClick(int page)
        {
            Page = page;
            dataGrid.ReloadServerData();
        }

        private void SearchReset()
        {
            InputTransaction = null;
            SelectedUser = null;
            SelectedOperateTarget = null;
            SelectedOperation = null;
            Page = 1;
            dataGrid.ReloadServerData();
        }

        private class AuditLogModel
        {
            public int Number { get; set; }

            public Guid Id { get; set; }

            public Guid TransactionId { get; set; }

            public string UserName { get; set; } = null!;

            public string? EntityName { get; set; }

            public int Operation { get; set; }

            public DateTime OperateTime { get; set; }
        }


        private class OperateTarget
        {
            public string EntityName { get; set; } = null!;

            public string DisplayName { get; set; } = null!;
        }

        private class Operator
        {
            public int Id { get; set; }

            public string UserName { get; set; } = null!;
        }
    }
}
