using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data
{
	public class DbContextInitialBackgroundService : BackgroundService
	{
		private readonly IDbContextFactory<BlazorAdminDbContext> _dbContextFactory;

		public DbContextInitialBackgroundService(
			IDbContextFactory<BlazorAdminDbContext> dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await Task.Yield();
			using var context = _dbContextFactory.CreateDbContext();
			context.InitialData();
		}
	}
}
