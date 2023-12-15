using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Im.Components
{
    public partial class ChatDialog
    {
        [CascadingParameter] MudDialogInstance? MudDialog { get; set; }

        private string _textValue = string.Empty;

        private string _messageValue = string.Empty;

        private object _selectedValue = 1;

        private MudListItem? _selectedItem;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            var scrollModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/BlazorAdmin.Im/js/scroll.js");
            await scrollModule.InvokeVoidAsync("scrollToBottom");
        }
    }
}
