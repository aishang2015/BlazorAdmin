using BlazorAdmin.Component.Pages;
using BlazorAdmin.Core.Extension;
using MudBlazor;
using static BlazorAdmin.Component.Pages.PagePagination;

namespace BlazorAdmin.Log.Pages.LoginLog
{
    public partial class LoginLog
    {
        private PageDataGrid<LoginLogModel> dataGrid = null!;

        private List<LoginLogModel> LoginLogs = new();

        private SearchObject searchObject = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        private async Task InitialAsync()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var query = context.LoginLogs.
                AndIfExist(searchObject.SearchedLoginName, l => l.UserName == searchObject.SearchedLoginName);
            LoginLogs = query.OrderByDescending(l => l.Id)
                .Skip((searchObject.Page - 1) * searchObject.Size).Take(searchObject.Size).ToList()
                .Select((l, i) => new LoginLogModel
                {
                    Id = l.Id,
                    Number = i + 1 + (searchObject.Page - 1) * searchObject.Size,
                    Agent = l.Agent,
                    Ip = l.Ip,
                    Time = l.Time,
                    IsSuccessd = l.IsSuccessd,
                    UserName = l.UserName,
                }).ToList();
            searchObject.Total = query.Count();
        }

        private async void PageChangedClick(int page)
        {
            searchObject.Page = page;
            dataGrid.ReloadServerData();
        }

        private async Task<GridData<LoginLogModel>> GetTableData(GridState<LoginLogModel> gridState)
        {
            await InitialAsync();
            return new GridData<LoginLogModel>() { TotalItems = searchObject.Total, Items = LoginLogs };
        }

        private void SearchReset()
        {
            searchObject = new();
            searchObject.Page = 1;
            dataGrid.ReloadServerData();
        }

        private record SearchObject : PaginationModel
        {
            public string? SearchedLoginName { get; set; }
        }

        private class LoginLogModel
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string UserName { get; set; } = null!;
            public DateTime Time { get; set; }
            public string? Agent { get; set; }
            public string? Ip { get; set; }
            public bool IsSuccessd { get; set; }
        }
    }
}
