using BlazorAdmin.Constants;
using BlazorAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data
{
	public class BlazorAdminDbContext : DbContext
	{
		private readonly string? _conn;

		public BlazorAdminDbContext(IConfiguration configuration)
		{
			_conn = configuration.GetConnectionString("Rbac");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);
			if (string.IsNullOrWhiteSpace(_conn))
			{
				throw new ArgumentNullException(nameof(_conn));
			}
			optionsBuilder.UseSqlite(_conn);
		}

		public DbSet<Setting> Settings { get; set; }

		public DbSet<User> Users { get; set; }

		public DbSet<Role> Roles { get; set; }

		public DbSet<UserRole> UserRoles { get; set; }

		public DbSet<Menu> Menus { get; set; }

		public DbSet<UserMenu> UserMenus { get; set; }


		public void InitialData()
		{
			Settings.AddRange(new Setting
			{
				Key = JwtConstant.JwtIssue,
				Value = "BlazorAdmin"
			}, new Setting
			{
				Key = JwtConstant.JwtAudience,
				Value = "Admin"
			}, new Setting
			{
				Key = JwtConstant.JwtSigningKey,
				Value = Guid.NewGuid().ToString()
			}, new Setting
			{
				Key = JwtConstant.JwtExpireMinute,
				Value = "720"
			});
			SaveChanges();
		}
	}
}
