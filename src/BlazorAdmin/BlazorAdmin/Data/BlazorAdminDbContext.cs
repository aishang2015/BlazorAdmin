using BlazorAdmin.Constants;
using BlazorAdmin.Data.Entities;
using BlazorAdmin.Shared.Components;
using Microsoft.EntityFrameworkCore;
using MudBlazor;

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

		public DbSet<RoleMenu> RoleMenus { get; set; }

		public void InitialData()
		{
			if (Database.EnsureCreated())
			{
				using var tran = Database.BeginTransaction();

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

				Menus.Add(new Menu
				{
					Name = "首页",
					Type = 1,
					Route = "/",
					Order = 1,
					Icon = Icons.Material.Filled.Home
				});
				var entry = Menus.Add(new Menu
				{
					Name = "权限",
					Type = 1,
					Route = "/",
					Order = 2,
					Icon = Icons.Material.Filled.Settings
				});
				Menus.Add(new Menu
				{
					Name = "关于",
					Type = 1,
					Route = "/",
					Order = 3,
					Icon = Icons.Material.Filled.TextFields
				});
				SaveChanges();

				Menus.Add(new Menu
				{
					ParentId = entry.Entity.Id,
					Name = "用户",
					Type = 1,
					Route = "/user",
					Order = 1,
					Icon = Icons.Material.Filled.Person
				});
				Menus.Add(new Menu
				{
					ParentId = entry.Entity.Id,
					Name = "角色",
					Type = 1,
					Route = "/role",
					Order = 2,
					Icon = Icons.Material.Filled.LockPerson
				});
				Menus.Add(new Menu
				{
					ParentId = entry.Entity.Id,
					Name = "菜单",
					Type = 1,
					Route = "/menu",
					Order = 3,
					Icon = Icons.Material.Filled.Menu
				});
				SaveChanges();
				tran.Commit();
			}
		}
	}
}
