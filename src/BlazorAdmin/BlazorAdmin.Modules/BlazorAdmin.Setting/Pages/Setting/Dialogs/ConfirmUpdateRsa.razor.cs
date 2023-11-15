using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Setting.Pages.Setting.Dialogs
{
	public partial class ConfirmUpdateRsa
	{

		[CascadingParameter] MudDialogInstance? MudDialog { get; set; }

		private void Confirm()
		{
			MudDialog?.Close(DialogResult.Ok(true));
		}
	}
}
