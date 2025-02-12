using BlazorAdmin.Ai.Pages.Config.Dialogs;
using BlazorAdmin.Component.Dialogs;
using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities.Ai;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using static BlazorAdmin.Component.Pages.PagePagination;

namespace BlazorAdmin.Ai.Pages.Config
{
    public partial class Config
    {
        private List<ConfigModel> Configs = new();
        private MudDataGrid<ConfigModel> dataGrid = null!;
        private SearchObject searchObject = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        private async Task<GridData<ConfigModel>> GetTableData(GridState<ConfigModel> state)
        {
            await InitialData();
            return new GridData<ConfigModel> { TotalItems = Configs.Count, Items = Configs };
        }

        private async Task InitialData()
        {
            using var context = await _dbFactory.CreateDbContextAsync();

            var query = context.AiConfigs
                .Where(c => !c.IsDeleted)
                .AndIfExist(searchObject.SearchModelName, c => c.ModelName.Contains(searchObject.SearchModelName!))
                .AndIfExist(searchObject.SearchEndpoint, c => c.Endpoint!.Contains(searchObject.SearchEndpoint!))
                .AndIfExist(searchObject.SearchCode, c => c.Code!.Contains(searchObject.SearchCode!));

            Configs = await query
                .OrderBy(c => c.Id)
                .Skip((searchObject.Page - 1) * searchObject.Size)
                .Take(searchObject.Size)
                .Select(c => new ConfigModel
                {
                    Id = c.Id,
                    ModelName = c.ModelName,
                    Endpoint = c.Endpoint,
                    ContextLength = c.ContextLength,
                    Description = c.Description,
                    Code = c.Code
                })
                .ToListAsync();

            searchObject.Total = await query.CountAsync();

            // 生成序号
            for (int i = 0; i < Configs.Count; i++)
            {
                Configs[i].Number = (searchObject.Page - 1) * searchObject.Size + i + 1;
            }
            StateHasChanged();
        }

        private async Task PageChangedClick(int page)
        {
            searchObject.Page = page;
            await dataGrid.ReloadServerData();
        }

        private async Task AddConfigClick()
        {
            var parameters = new DialogParameters();
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var dialog = await _dialogService.ShowAsync<EditConfigDialog>(Loc["AIConfigPage_CreateTitle"], parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await dataGrid.ReloadServerData();
            }
        }

        private async Task EditConfigClick(int configId)
        {
            var parameters = new DialogParameters { { "ConfigId", configId } };
            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium };
            var dialog = await _dialogService.ShowAsync<EditConfigDialog>(Loc["AIConfigPage_EditTitle"], parameters, options);
            var result = await dialog.Result;
            if (!result.Canceled)
            {
                await dataGrid.ReloadServerData();
            }
        }

        private async Task DeleteConfigClick(int configId)
        {
            await _dialogService.ShowDeleteDialog(Loc["AIConfigPage_DeleteTitle"], null, async (e) =>
            {
                using var context = await _dbFactory.CreateDbContextAsync();
                var config = await context.AiConfigs.FindAsync(configId);
                if (config != null)
                {
                    config.IsDeleted = true;
                    await context.SaveChangesAsync();
                    _snackbarService.Add(Loc["AIConfigPage_DeleteSuccess"], Severity.Success);
                }
                await dataGrid.ReloadServerData();
            });
        }

        private async Task TestAi(int configId)
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var config = context.AiConfigs.Find(configId);
            var result = await _aiHelper.TestAiConfig(config.ModelName, config.ApiKey, config.Endpoint, configId);
            if (result.IsSuccess)
            {
                _snackbarService.Add(Loc["AIConfigPage_TestSuccess"], Severity.Success);
            }
            else
            {
                _snackbarService.Add(result.ErrorMessage, Severity.Error);
            }
        }

        private void SearchReset()
        {
            searchObject = new SearchObject { Page = 1 };
            dataGrid.ReloadServerData();
        }

        public record SearchObject : PaginationModel
        {
            public string? SearchModelName { get; set; }
            public string? SearchEndpoint { get; set; }
            public string? SearchCode { get; set; }
        }

        public class ConfigModel
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public string ModelName { get; set; } = null!;
            public string? Endpoint { get; set; }
            public int ContextLength { get; set; }
            public string? Description { get; set; }
            public string? Code { get; set; }
        }
    }
}
