using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorAdmin.Web.Components.Shared
{
    public class LoggerErrorBoundary : ErrorBoundary
    {
        [Inject] ILogger<LoggerErrorBoundary> Logger { get; set; } = null!;

        protected override async Task OnErrorAsync(Exception exception)
        {
            Logger.LogError(exception, string.Empty);
            await base.OnErrorAsync(exception);
        }
    }
}
