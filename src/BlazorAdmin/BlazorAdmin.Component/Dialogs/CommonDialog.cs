using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Component.Dialogs
{
	public static class CommonDialog
	{
		public delegate Task ConfirmDelegate(CommonDialogEventArgs args);

		public static async Task<bool> ShowDeleteDialog(this IDialogService dialogService,
			string? title, string? confirmButtonText, ConfirmDelegate confirmCallBackMethod)
		{
			var confirmCallBack = new EventCallback<CommonDialogEventArgs>(null,
				async (CommonDialogEventArgs e) => await confirmCallBackMethod(e));
			var parameters = new DialogParameters
			{
				{"Title", title},
				{"ConfirmButtonText", confirmButtonText},
				{"ConfirmCallBack", confirmCallBack }
			};
			var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true, };
			var result = await dialogService.Show<CommonDeleteDialog>(string.Empty, parameters, options).Result;
			return !result.Canceled;
		}
	}
}
