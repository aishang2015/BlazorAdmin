using BlazorAdmin.Data.Constants;
using BlazorAdmin.Web.Components.Shared.Dialogs.Layout;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Routing;
using MudBlazor;
using System.Globalization;
using System.Text.Json;
using static BlazorAdmin.Rbac.Components.NavItemMenu;
using static MudBlazor.CategoryTypes;

namespace BlazorAdmin.Web.Components.Layout
{
    public partial class AuthorizedLayout
    {
        [Parameter] public RenderFragment? Child { get; set; }

        bool _drawerOpen = true;

        bool visible;

        DialogOptions dialogOptions = new() { NoHeader = true };

        string _userName = string.Empty;

        string? _avatar = string.Empty;

        CultureInfo _culture;

        private List<TabView> _userTabs = new();

        private int _selectedTabIndex = 0;

        private Shared.NavMenu _navMenuRef = new();

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            _culture = CultureInfo.CurrentCulture;

            var currentUri = _navManager.Uri.Substring(_navManager.BaseUri.Length - 1);
            if (currentUri.Contains('?'))
            {
                currentUri = currentUri.Substring(0, currentUri.IndexOf('?'));
            }

            var couldAccess = await _accessService.CheckUriCanAccess(currentUri);
            if (!couldAccess)
            {
                _navManager.NavigateTo("/noauthorized");
            }
            else
            {
                var user = await _stateProvider.GetAuthenticationStateAsync();
                using var context = await _dbFactory.CreateDbContextAsync();
                var userInfo = context.Users.Find(user.User.GetUserId());
                _userName = userInfo.RealName;
                _avatar = userInfo.Avatar;
            }

        }

        void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        private void CultureChanged(string culture)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                var uri = new Uri(_navManager.Uri)
                        .GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
                var cultureEscaped = Uri.EscapeDataString(culture);
                //var uriEscaped = Uri.EscapeDataString(uri);

                _navManager.NavigateTo(
                    $"api/Culture/Set?culture={cultureEscaped}&redirectUri={uri}",
                    forceLoad: true);
            }
        }

        private async Task ShowUserSettings()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            await _dialogService.Show<ProfileSetting>(string.Empty, parameters, options).Result;

            using var context = await _dbFactory.CreateDbContextAsync();
            var user = await _stateProvider.GetAuthenticationStateAsync();
            var userInfo = context.Users.Find(user.User.GetUserId());
            _userName = userInfo.RealName;
            _avatar = userInfo.Avatar;
            StateHasChanged();
        }


        private async Task LogoutClick()
        {
            await _localStorage.DeleteAsync(CommonConstant.UserId);
            await _localStorage.DeleteAsync(CommonConstant.UserToken);
            await _localStorage.DeleteAsync("tabs");
            await _authService.SetCurrentUser();

            _navManager.NavigateTo("/login", true);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                var result = await _localStorage.GetAsync<string>("tabs");
                if (result.Success)
                {
                    _userTabs = JsonSerializer.Deserialize<List<TabView>>(result.Value);
                    var index = _userTabs.FindIndex(t => _navManager.Uri.EndsWith(t.Route));
                    if (index == -1)
                    {
                        await NavigateTo(_navMenuRef.NavMenuItems.First());
                    }
                    else
                    {
                        _selectedTabIndex = index;
                    }
                    StateHasChanged();
                }
                else
                {
                    await NavigateTo(_navMenuRef.NavMenuItems.First());
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
            await _localStorage.SetAsync("tabs", JsonSerializer.Serialize(_userTabs));
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
                    await NavigateTo(_navMenuRef.NavMenuItems.First());
                    _navManager.NavigateTo("/");
                }
            }
            await _localStorage.SetAsync("tabs", JsonSerializer.Serialize(_userTabs));
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
