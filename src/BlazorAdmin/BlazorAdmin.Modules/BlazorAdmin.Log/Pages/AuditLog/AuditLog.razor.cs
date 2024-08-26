using BlazorAdmin.Component.Pages;
using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data.Attributes;
using BlazorAdmin.Log.Pages.AuditLog.Dialogs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MudBlazor;
using System.Reflection;
using static BlazorAdmin.Component.Pages.PagePagination;

namespace BlazorAdmin.Log.Pages.AuditLog
{
    public partial class AuditLog
    {

        private PageDataGrid<AuditLogModel> dataGrid = null!;

        private List<AuditLogModel> AuditLogs = new();

        private List<Operator> Operators = new();

        private List<OperateTarget> OperateTargets = new();

        private SearchObject searchObject = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        private async Task InitialAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var auditLogQuery = context.AuditLogs
                .AndIf(!string.IsNullOrEmpty(searchObject.InputTransaction) && Guid.TryParse(searchObject.InputTransaction, out Guid inputGuid),
                        q => q.TransactionId == Guid.Parse(searchObject.InputTransaction!))
                .AndIfExist(searchObject.SelectedUser, q => q.UserId == int.Parse(searchObject.SelectedUser!))
                .AndIfExist(searchObject.SelectedOperateTarget, q => q.EntityName == searchObject.SelectedOperateTarget)
                .AndIfExist(searchObject.SelectedOperation, q => q.Operation == int.Parse(searchObject.SelectedOperation!));

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
            AuditLogs = query.Skip((searchObject.Page - 1) * searchObject.Size).Take(searchObject.Size).ToList()
                .Select((d, i) => new AuditLogModel
                {
                    Number = i + (searchObject.Page - 1) * searchObject.Size + 1,
                    Id = d.Id,
                    TransactionId = d.TransactionId,
                    EntityName = model.FindEntityType(d.EntityName)?.GetComment(),
                    OperateTime = d.OperateTime,
                    Operation = d.Operation,
                    UserName = d.RealName
                }).ToList();
            searchObject.Total = query.Count();

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
            searchObject.Page = page;
            dataGrid.ReloadServerData();
        }

        private void SearchReset()
        {
            searchObject = new SearchObject();
            searchObject.Page = 1;
            dataGrid.ReloadServerData();
        }

        private record SearchObject : PaginationModel
        {
            public string? InputTransaction { get; set; }

            public string? SelectedUser { get; set; }

            public string? SelectedOperateTarget { get; set; }

            public string? SelectedOperation { get; set; }
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
