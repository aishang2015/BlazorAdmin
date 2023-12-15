using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Im.Components
{
    public partial class Chat
    {
        private bool _haveMsg = false;

        private async Task ViewIm()
        {
            var parameters = new DialogParameters { };
            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraLarge, NoHeader = true };
            var result = await _dialogService.Show<ChatDialog>(null, parameters, options).Result;
        }
    }
}
