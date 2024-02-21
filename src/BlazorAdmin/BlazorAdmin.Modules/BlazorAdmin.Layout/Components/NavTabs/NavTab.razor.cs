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

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _layoutState.NavToEvent += async (i) => await NavigateTo(i);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                var result = await _localStorage.GetAsync<string>(CommonConstant.Tabs);
                if (result.Success)
                {
                    _userTabs = JsonSerializer.Deserialize<List<TabView>>(result.Value);
                    var index = _userTabs.FindIndex(t => _navManager.Uri.EndsWith(t.Route));
                    if (index == -1)
                    {
                        //await NavigateTo(_navMenuRef.NavMenuItems.First());
                    }
                    else
                    {
                        _selectedTabIndex = index;
                    }
                    StateHasChanged();
                }
                else
                {
                    //await NavigateTo(_navMenuRef.NavMenuItems.First());
                }
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
            await _localStorage.SetAsync(CommonConstant.Tabs, JsonSerializer.Serialize(_userTabs));
            StateHasChanged();
        }

        private async Task TabClick(TabView tab)
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
                else if (!_userTabs.Any())
                {
                    // await NavigateTo(_navMenuRef.NavMenuItems.First());
                    _navManager.NavigateTo("/");
                }
            }
            await _localStorage.SetAsync(CommonConstant.Tabs, JsonSerializer.Serialize(_userTabs));
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
