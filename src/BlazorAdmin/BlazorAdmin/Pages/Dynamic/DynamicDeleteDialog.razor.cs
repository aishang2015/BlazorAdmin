using BlazorAdmin.Core.Dynamic;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BlazorAdmin.Pages.Dynamic
{
	public partial class DynamicDeleteDialog
	{
		[CascadingParameter]
		MudDialogInstance? MudDialog { get; set; }

		[Parameter]
		public DynamicEntityInfo EntityInfo { get; set; } = null!;

		[Parameter]
		public dynamic UtilInstance { get; set; } = null!;

		[Parameter]
		public object[] Keys { get; set; } = new object[0];

		private async Task ConfirmDelete()
		{
			var methodInfo = UtilInstance.GetType().GetMethod($"Delete{EntityInfo!.EntityName}");
			using var context = await _dbFactory.CreateDbContextAsync();
			methodInfo!.Invoke(UtilInstance, new object[] { context, Keys! });
			MudDialog?.Close(DialogResult.Ok(true));
		}
	}
}
