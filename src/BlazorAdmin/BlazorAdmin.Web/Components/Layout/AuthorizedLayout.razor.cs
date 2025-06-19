using BlazorAdmin.Layout.States;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorAdmin.Web.Components.Layout
{
    public partial class AuthorizedLayout : IDisposable
    {
        [Inject] private LayoutState _layoutState { get; set; } = null!;
        [Inject] private NavigationManager _navigationManager { get; set; } = null!;

        [Parameter] public RenderFragment? Child { get; set; }

        protected override void OnParametersSet()
        {
            // Subscribe to events first
            _layoutState.TabsChangedEvent -= OnTabsChanged; // Ensure not subscribed multiple times if OnParametersSet is called more than once
            _layoutState.TabsChangedEvent += OnTabsChanged;
            _layoutState.ActiveTabChangedEvent -= OnActiveTabChanged; // Ensure not subscribed multiple times
            _layoutState.ActiveTabChangedEvent += OnActiveTabChanged;

            if (!_layoutState.OpenTabs.Any())
            {
                var currentUri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
                var currentRoute = currentUri.AbsolutePath;

                // Avoid creating tab for the base path or if already processed
                if (!string.IsNullOrEmpty(currentRoute) && currentRoute != "/" )
                {
                    // Attempt to infer title from the last segment of the route
                    var titleSegments = currentRoute.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    var title = titleSegments.Any() ? titleSegments.Last() : "Page";

                    // Basic capitalization
                    if (!string.IsNullOrEmpty(title))
                    {
                        title = char.ToUpperInvariant(title[0]) + title.Substring(1).ToLowerInvariant();
                    }

                    // Check if a tab for this route already exists, perhaps due to a quick redirect or other logic
                    if (!_layoutState.OpenTabs.Any(t => t.Route == currentRoute))
                    {
                         _layoutState.OpenTab(currentRoute, title);
                    }
                }
            }
        }

        private void OnTabsChanged()
        {
            StateHasChanged();
        }

        private void OnActiveTabChanged()
        {
            StateHasChanged();
        }

        protected void CloseTabClicked(TabInfo tab)
        {
            _layoutState.CloseTab(tab);
        }

        protected void OnMudTabActivated(TabInfo tab)
        {
            _layoutState.SetActiveTab(tab);
            if (_navigationManager.Uri != _navigationManager.BaseUri + tab.Route.TrimStart('/'))
            {
                _navigationManager.NavigateTo(tab.Route);
            }
        }

        protected void OnActivePanelIndexChanged(int index)
        {
            if (index >= 0 && index < _layoutState.OpenTabs.Count)
            {
                var tab = _layoutState.OpenTabs[index];
                OnMudTabActivated(tab);
            }
        }

        public void Dispose()
        {
            _layoutState.TabsChangedEvent -= OnTabsChanged;
            _layoutState.ActiveTabChangedEvent -= OnActiveTabChanged;
            GC.SuppressFinalize(this);
        }
    }
}
