using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Pages.Dynamic
{
	public partial class DynamicEditDialog
	{
		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }
	}
}
