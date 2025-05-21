using Microsoft.Extensions.Hosting;

namespace BlazorAdmin.Core.Services
{
    public abstract class LoopBackgroundService : BackgroundService
    {
        protected int IntervalMilliSeconds { get; set; } = 1000;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(IntervalMilliSeconds, stoppingToken);
                await ExecuteLoopBodyAsync(stoppingToken);
            }
        }

        protected abstract Task ExecuteLoopBodyAsync(CancellationToken stoppingToken);
    }

}
