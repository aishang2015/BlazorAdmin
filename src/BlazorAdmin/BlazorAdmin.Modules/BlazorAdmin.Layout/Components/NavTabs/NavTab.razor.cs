using BlazorAdmin.Core.Extension;
using BlazorAdmin.Data.Constants;
using MudBlazor;
using System.Text.Json;
using static BlazorAdmin.Layout.Components.NavMenus.NavItemMenu;

namespace BlazorAdmin.Layout.Components.NavTabs
{
    public partial class NavTab
    {
        private List<TabView> _userTabs = new();

        private int _selectedTabIndex = 0;

        protected override async void OnInitialized()
        {
            base.OnInitialized();
            _layoutState.NavToEvent += async (i) => await NavigateTo(i);

            var state = await _stateProvider.GetAuthenticationStateAsync();
            using var context = await _dbFactory.CreateDbContextAsync();
            var userTabs = context.UserSettings
                .FirstOrDefault(s => s.UserId == state.User.GetUserId() && s.Key == CommonConstant.UserTabs)
                ?.Value;
            if (userTabs != null)
            {
                _userTabs = JsonSerializer.Deserialize<List<TabView>>(userTabs);
                var index = _userTabs.FindIndex(t => _navManager.Uri.EndsWith(t.Route));
                if (index == -1)
                {
                    await NavToDefault();
                }
                else
                {
                    _selectedTabIndex = index;
                }
            }
            else
            {
                await NavToDefault();
            }
        }

        private async Task NavToDefault()
        {
            using var context = await _dbFactory.CreateDbContextAsync();
            var menuList = context.Menus.Where(m => m.Type == 1).ToList();

            var canAccessedMenus = await _accessService.GetCanAccessedMenus();
            var defaultMenu = menuList.Where(m => canAccessedMenus.Contains(m.Id)).Select(m => new NavMenuItem
            {
                MenuName = m.Name,
                Route = m.Route,
            }).FirstOrDefault();
            if (defaultMenu != null)
            {
                await NavigateTo(defaultMenu);
            }
        }

        private async Task NavigateTo(NavMenuItem route)
        {
            if (_userTabs.All(t => t.Route != route.Route))
            {
                _userTabs.Add(new TabView
                {
                    Id = Guid.NewGuid(),
                    Label = route.MenuName!,
                    Route = route.Route,
                    ShowCloseIcon = true
                });
            }
            var index = _userTabs.FindIndex(t => t.Route == route.Route);
            _selectedTabIndex = index;
            await SaveUserTabsAsync();
            StateHasChanged();
        }

        private void TabClick(TabView tab)
        {
            _navManager.NavigateTo(tab.Route);
        }

        private async Task OnTabClose(MudTabPanel panel)
        {
            var tabView = _userTabs.FirstOrDefault(t => t.Id == (Guid)panel.ID);
            if (tabView is not null)
            {
                _userTabs.Remove(tabView);
                if (_userTabs.Any())
                {
                    if (_navManager.Uri.EndsWith(tabView.Route))
                    {
                        // active last userTab = 
                        var lastUserTab = _userTabs.Last();
                        _selectedTabIndex = _userTabs.Count - 1;
                        _navManager.NavigateTo(lastUserTab.Route);
                    }
                    else
                    {
                        var index = _userTabs.FindIndex(t => _navManager.Uri.EndsWith(t.Route));
                        _selectedTabIndex = index;
                    }
                }
            }
            await SaveUserTabsAsync();
        }

        private async Task SaveUserTabsAsync()
        {
            var state = await _stateProvider.GetAuthenticationStateAsync();
            using var context = await _dbFactory.CreateDbContextAsync();
            var userTabSetting = context.UserSettings
                .FirstOrDefault(s => s.UserId == state.User.GetUserId() && s.Key == CommonConstant.UserTabs);
            if (userTabSetting == null)
            {
                context.UserSettings.Add(new Data.Entities.Setting.UserSetting
                {
                    UserId = state.User.GetUserId(),
                    Key = CommonConstant.UserTabs,
                    Value = JsonSerializer.Serialize(_userTabs)
                });
            }
            else
            {
                userTabSetting.Value = JsonSerializer.Serialize(_userTabs);
                context.UserSettings.Update(userTabSetting);
            }
            context.SaveChanges();
        }



        public class TabView
        {
            public string Label { get; set; } = null!;
            public Guid Id { get; set; }
            public bool ShowCloseIcon { get; set; } = true;

            public string Route { get; set; } = null!;
        }
    }
}
