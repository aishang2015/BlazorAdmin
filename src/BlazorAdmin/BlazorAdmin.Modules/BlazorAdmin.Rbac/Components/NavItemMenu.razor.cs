using Microsoft.AspNetCore.Components;

namespace BlazorAdmin.Rbac.Components
{
    public partial class NavItemMenu
    {
        [Parameter] public HashSet<NavMenuItem> NavMenuItems { get; set; } = new();

        [Parameter] public EventCallback<NavMenuItem> NavTo { get; set; }

        private bool _shouldRender = false;

        protected override bool ShouldRender() => _shouldRender;

        private async Task NavClick(NavMenuItem item)
        {
            await NavTo.InvokeAsync(item);
        }

        public record NavMenuItem
        {
            public string? MenuIcon { get; set; }

            public string? MenuName { get; set; }

            public string? Route { get; set; }

            public HashSet<NavMenuItem> Childs { get; set; } = new();
        }
    }
}
