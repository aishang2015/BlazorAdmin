using static BlazorAdmin.Layout.Components.NavMenus.NavItemMenu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorAdmin.Layout.States
{
    public class TabInfo
    {
        public string Id { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Route { get; set; } = null!;
        public Type? ComponentType { get; set; }
    }

    public class BreadcrumbItem
    {
        public string Text { get; set; }
        public string? Href { get; set; } // Null Href for non-clickable items (e.g., current page)
        public bool Disabled { get; set; }

        public BreadcrumbItem(string text, string? href = null, bool disabled = false)
        {
            Text = text;
            Href = href;
            Disabled = disabled;
        }
    }

    public class LayoutState
    {
        private HashSet<NavMenuItem> AllNavMenuItems { get; set; } = new HashSet<NavMenuItem>();
        public List<BreadcrumbItem> CurrentBreadcrumbs { get; private set; } = new List<BreadcrumbItem>();
        public event Action? BreadcrumbsChangedEvent;

        private bool _navIsOpen = true;

        public event Action? NavIsOpenEvent;

        public bool NavIsOpen
        {
            get => _navIsOpen;
            set
            {
                _navIsOpen = value;
                NavIsOpenEvent?.Invoke();
            }
        }

        public List<TabInfo> OpenTabs { get; private set; } = new List<TabInfo>();
        public TabInfo? ActiveTab { get; private set; }

        public event Action? TabsChangedEvent;
        public event Action? ActiveTabChangedEvent;

        public void SetAvailableNavItems(HashSet<NavMenuItem> items)
        {
            AllNavMenuItems = items;
        }

        private bool FindBreadcrumbPath(HashSet<NavMenuItem> itemsToSearch, string targetRoute, List<NavMenuItem> path)
        {
            foreach (var item in itemsToSearch.OrderBy(i => i.MenuName)) // OrderBy for deterministic path if multiple routes could match parts
            {
                if (item.Route == targetRoute)
                {
                    path.Add(item);
                    return true;
                }

                if (item.Childs != null && item.Childs.Any())
                {
                    path.Add(item);
                    if (FindBreadcrumbPath(item.Childs, targetRoute, path))
                    {
                        return true;
                    }
                    path.RemoveAt(path.Count - 1); // Backtrack
                }
            }
            return false;
        }

        private void UpdateBreadcrumbs(TabInfo? activeTab)
        {
            CurrentBreadcrumbs.Clear();
            if (activeTab == null || string.IsNullOrEmpty(activeTab.Route) || !AllNavMenuItems.Any())
            {
                BreadcrumbsChangedEvent?.Invoke();
                return;
            }

            var foundPath = new List<NavMenuItem>();
            FindBreadcrumbPath(AllNavMenuItems, activeTab.Route, foundPath);

            if (foundPath.Any())
            {
                for (int i = 0; i < foundPath.Count; i++)
                {
                    var navItem = foundPath[i];
                    bool isLast = i == foundPath.Count - 1;
                    CurrentBreadcrumbs.Add(new BreadcrumbItem(
                        text: navItem.MenuName!,
                        href: isLast || string.IsNullOrEmpty(navItem.Route) ? null : navItem.Route,
                        disabled: isLast
                    ));
                }
            }
            else if (!string.IsNullOrEmpty(activeTab.Title)) // Fallback for routes not in NavMenu (e.g. direct nav to a page not in menu)
            {
                CurrentBreadcrumbs.Add(new BreadcrumbItem(activeTab.Title, null, true));
            }

            BreadcrumbsChangedEvent?.Invoke();
        }

        private void OpenOrActivateTab(string route, string title, Type? componentType = null)
        {
            var existingTab = OpenTabs.FirstOrDefault(t => t.Route == route);
            if (existingTab != null)
            {
                SetActiveTab(existingTab);
            }
            else
            {
                var newTab = new TabInfo
                {
                    Id = route,
                    Title = title,
                    Route = route,
                    ComponentType = componentType
                };
                OpenTabs.Add(newTab);
                SetActiveTab(newTab);
                TabsChangedEvent?.Invoke();
            }
        }

        public void OpenTab(NavMenuItem item)
        {
            OpenOrActivateTab(item.Route!, item.MenuName!);
        }

        public void OpenTab(string route, string title, Type? componentType = null)
        {
            OpenOrActivateTab(route, title, componentType);
        }

        public void CloseTab(TabInfo tabToClose)
        {
            int tabIndex = OpenTabs.IndexOf(tabToClose);
            if (tabIndex >= 0)
            {
                OpenTabs.Remove(tabToClose);
                if (ActiveTab == tabToClose)
                {
                    if (OpenTabs.Any())
                    {
                        ActiveTab = OpenTabs.ElementAtOrDefault(Math.Max(0, tabIndex - 1));
                    }
                    else
                    {
                        ActiveTab = null;
                    }
                    ActiveTabChangedEvent?.Invoke();
                }
                TabsChangedEvent?.Invoke();
            }
        }

        public void SetActiveTab(TabInfo? tab)
        {
            if (ActiveTab != tab)
            {
                ActiveTab = tab;
                ActiveTabChangedEvent?.Invoke();
                UpdateBreadcrumbs(tab);
            }
            else if (ActiveTab == tab && tab != null) // Ensure breadcrumbs update even if tab is "re-activated"
            {
                UpdateBreadcrumbs(tab);
            }
        }

        // public event Action<NavMenuItem>? NavToEvent;
        // public void NavTo(NavMenuItem item) => NavToEvent?.Invoke(item);
    }
}
