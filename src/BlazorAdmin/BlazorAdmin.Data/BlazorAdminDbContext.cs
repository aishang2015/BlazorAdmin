using BlazorAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data
{
	public class BlazorAdminDbContext : DbContext
	{
		public BlazorAdminDbContext(DbContextOptions<BlazorAdminDbContext> options) : base(options)
		{
		}

		public DbSet<Setting> Settings { get; set; }

		public DbSet<User> Users { get; set; }

		public DbSet<Role> Roles { get; set; }

		public DbSet<UserRole> UserRoles { get; set; }

		public DbSet<Menu> Menus { get; set; }

		public DbSet<RoleMenu> RoleMenus { get; set; }

		public DbSet<AuditLog> AuditLogs { get; set; }

		public DbSet<AuditLogDetail> AuditLogDetails { get; set; }

		public DbSet<LoginLog> LoginLogs { get; set; }
	}
}
