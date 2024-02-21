using Microsoft.AspNetCore.Components;

namespace BlazorAdmin.Web.Components.Layout
{
    public partial class AuthorizedLayout
    {
        [Parameter] public RenderFragment? Child { get; set; }
    }
}
