using Microsoft.AspNetCore.Components;

namespace BlazorAdmin.Rbac.Components
{
    public partial class NavItemMenu
    {
        [Parameter] public HashSet<NavMenuItem> NavMenuItems { get; set; } = new();

        public record NavMenuItem
        {
            public string? MenuIcon { get; set; }

            public string? MenuName { get; set; }

            public string? Route { get; set; }

            public HashSet<NavMenuItem> Childs { get; set; } = new();
        }
    }
}
