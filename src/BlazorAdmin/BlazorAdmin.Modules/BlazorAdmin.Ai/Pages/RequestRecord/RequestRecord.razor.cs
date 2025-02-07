using BlazorAdmin.Component.Pages;
using BlazorAdmin.Data.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using static BlazorAdmin.Component.Pages.PagePagination;
using BlazorAdmin.Ai.Pages.RequestRecord.Dialogs;
using BlazorAdmin.Core.Extension;

namespace BlazorAdmin.Ai.Pages.RequestRecord
{
    public partial class RequestRecord
    {
        private List<RequestRecordModel> Records = new();

        private MudDataGrid<RequestRecordModel> dataGrid = null!;

        private SearchObject searchObject = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        private async Task<GridData<RequestRecordModel>> GetTableData(GridState<RequestRecordModel> state)
        {
            await InitialData();
            return new GridData<RequestRecordModel> { TotalItems = Records.Count, Items = Records };
        }

        private async Task InitialData()
        {
            using var context = await _dbFactory.CreateDbContextAsync();

            var query = context.AiRequestRecords
                .AndIfExist(searchObject.StartTime, r => r.RequestTime >= searchObject.StartTime)
                .AndIfExist(searchObject.EndTime, r => r.RequestTime < searchObject.EndTime.Value.AddDays(1));


            Records = await query
                .OrderByDescending(r => r.Id)
                .Skip((searchObject.Page - 1) * searchObject.Size)
                .Take(searchObject.Size)
                .Select(r => new RequestRecordModel
                {
                    Id = r.Id,
                    RequestTime = r.RequestTime,
                    RequestTokens = r.RequestTokens,
                    ResponseTokens = r.ResponseTokens,
                    TotalPrice = r.TotalPrice
                })
                .ToListAsync();

            searchObject.Total = await query.CountAsync();

            // 生成序号
            for (int i = 0; i < Records.Count; i++)
            {
                Records[i].Number = (searchObject.Page - 1) * searchObject.Size + i + 1;
            }
            StateHasChanged();
        }

        private async Task PageChangedClick(int page)
        {
            searchObject.Page = page;
            await dataGrid.ReloadServerData();
        }

        private void SearchReset()
        {
            searchObject = new SearchObject { Page = 1 };
            dataGrid.ReloadServerData();
        }

        private async Task ShowDetail(int recordId)
        {
            var parameters = new DialogParameters { { "RecordId", recordId } };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };
            await _dialogService.ShowAsync<DetailDialog>(Loc["AIRequestRecord_DetailTitle"], parameters, options);
        }

        private record SearchObject : PaginationModel
        {
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
        }

        private class RequestRecordModel
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public DateTime RequestTime { get; set; }
            public int RequestTokens { get; set; }
            public int ResponseTokens { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}