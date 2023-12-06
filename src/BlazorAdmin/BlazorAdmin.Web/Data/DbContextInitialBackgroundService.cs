using BlazorAdmin.Core.Data;
using BlazorAdmin.Data;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Web.Data
{
    public class DbContextInitialBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public DbContextInitialBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            using var scope = _serviceProvider.CreateScope();
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlazorAdminDbContext>>();
            using var context = dbContextFactory.CreateDbContext();
            context.InitialData();
        }
    }
}
