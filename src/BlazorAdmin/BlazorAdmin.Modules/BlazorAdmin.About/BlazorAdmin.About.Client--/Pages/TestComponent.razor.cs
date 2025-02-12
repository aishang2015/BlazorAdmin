namespace BlazorAdmin.About.Client.Pages
{
    public partial class TestComponent
    {
        string code = """
            @page "/路由"

            @rendermode RenderMode.InteractiveServer

            <PageHeader Title="标题">
                <MudIconButton Size="MudBlazor.Size.Medium" Icon="@Icons.Material.Filled.Search" OnClick="()=>dataGrid.ReloadServerData()"
                               Variant="Variant.Outlined" Color="Color.Primary" Class="mr-2"></MudIconButton>
                <MudTooltip Text="重置搜索" Color="Color.Primary">
                    <MudIconButton Size="MudBlazor.Size.Medium" Icon="@Icons.Material.Filled.SearchOff" OnClick="SearchReset"
                                   Variant="Variant.Outlined" Color="Color.Primary"></MudIconButton>
                </MudTooltip>
            </PageHeader>

            <MudDataGrid Dense=PageDataGridConfig.Dense Filterable=PageDataGridConfig.Filterable ColumnResizeMode=PageDataGridConfig.ColumnResizeMode
                         SortMode=PageDataGridConfig.SortMode Groupable=PageDataGridConfig.Groupable Virtualize=PageDataGridConfig.Virtualize
                         FixedHeader=PageDataGridConfig.FixedHeader Elevation=PageDataGridConfig.Elevation Outlined=PageDataGridConfig.Outlined
                         Style="@PageDataGridConfig.Style" HorizontalScrollbar="PageDataGridConfig.HorizontalScrollbar"
                         ServerData="GetTableData" @ref="dataGrid" T="TableModel">
                <Columns>
                    <TemplateColumn Title="操作" HeaderStyle="white-space:nowrap;">
                        <CellTemplate>
                        </CellTemplate>
                    </TemplateColumn>
                </Columns>
            </MudDataGrid>

            <PagePagination PageInfo="searchObject" PageChangedClick="PageChangedClick"></PagePagination>

            """;

        string code2 = """
            private MudDataGrid<TableModel> dataGrid = null!;

            private List<TableModel> TableData = new();

            private SearchObject searchObject = new();

            protected override async Task OnInitializedAsync()
            {
                await base.OnInitializedAsync();
            }

            private async Task InitialAsync()
            {
                using var context = await _dbFactory.CreateDbContextAsync();
             
                searchObject.Total = 0;
                StateHasChanged();
            }

            private async Task PageChangedClick(int page)
            {
                searchObject.Page = page;
                await dataGrid.ReloadServerData();
            }

            private async Task<GridData<TableModel>> GetTableData(GridState<TableModel> gridState)
            {
                await InitialAsync();
                return new GridData<TableModel>() { TotalItems = searchObject.Total, Items = TableData };
            }

            private void SearchReset()
            {
                searchObject = new();
                searchObject.Page = 1;
                dataGrid.ReloadServerData();
            }

            private record SearchObject : PaginationModel
            {
            }

            private class TableModel
            {
                public int Id { get; set; }
            }


            """;
    }
}
