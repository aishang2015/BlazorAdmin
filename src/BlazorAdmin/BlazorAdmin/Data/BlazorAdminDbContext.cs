using BlazorAdmin.Constants;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data.Entities;
using BlazorAdmin.Shared.Components;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MudBlazor;

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

				Menus.Add(new Menu { Name = "首页", Type = 1, Route = "/", Order = 1, Icon = Icons.Material.Filled.Home });
				var entry = Menus.Add(new Menu { Name = "日志", Type = 1, Route = "/", Order = 2, Icon = Icons.Material.Filled.Info });
				var entry2 = Menus.Add(new Menu { Name = "权限", Type = 1, Route = "/", Order = 3, Icon = Icons.Material.Filled.Settings });
				Menus.Add(new Menu { Name = "关于", Type = 1, Route = "/", Order = 4, Icon = Icons.Material.Filled.TextFields });
				SaveChanges();

				Menus.Add(new Menu { ParentId = entry.Entity.Id, Name = "审计", Type = 1, Route = "/auditLog", Order = 1, Icon = Icons.Material.Filled.Verified });
				Menus.Add(new Menu { ParentId = entry.Entity.Id, Name = "登录", Type = 1, Route = "/loginLog", Order = 2, Icon = Icons.Material.Filled.Login });
				SaveChanges();

				Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "用户", Type = 1, Route = "/user", Order = 1, Icon = Icons.Material.Filled.Person });
				Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "角色", Type = 1, Route = "/role", Order = 2, Icon = Icons.Material.Filled.LockPerson });
				Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "菜单", Type = 1, Route = "/menu", Order = 3, Icon = Icons.Material.Filled.Menu });
				SaveChanges();

				var roleEntry = Roles.Add(new Role { IsEnabled = true, Name = "Admin" });
				SaveChanges();

				var userEntry = Users.Add(new User { Name = "BlazorAdmin", IsEnabled = true, PasswordHash = HashHelper.HashPassword("BlazorAdmin") });
				SaveChanges();

				UserRoles.Add(new UserRole
				{
					UserId = userEntry.Entity.Id,
					RoleId = roleEntry.Entity.Id
				});
				SaveChanges();

				RoleMenus.AddRange(Menus.Select(m => m.Id).Select(id => new RoleMenu
				{
					MenuId = id,
					RoleId = roleEntry.Entity.Id
				}));
				SaveChanges();

				tran.Commit();
			}
		}


		public async Task CustomSaveChangesAsync(AuthenticationStateProvider provider)
		{
			var userState = await provider.GetAuthenticationStateAsync();
			if (userState.User.Identity != null && userState.User.Identity.IsAuthenticated)
			{
				var userId = userState.User.GetUserId();
				var entries = ChangeTracker.Entries()
					.Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
					.ToList();

				var transactionId = Database.CurrentTransaction?.TransactionId ?? Guid.NewGuid();

				foreach (var entry in entries)
				{
					var logId = Guid.NewGuid();
					var auditLog = new AuditLog()
					{
						Id = logId,
						TransactionId = transactionId,
						OperateTime = DateTime.UtcNow,
						EntityName = entry.Metadata.Name,
						Operation = (int)entry.State,
						UserId = userId
					};
					AuditLogs.Add(auditLog);


					if (entry.State is EntityState.Modified)
					{
						var modifiedProperties = entry.Properties.Where(p => p.IsModified);
						foreach (var property in modifiedProperties)
						{
							var auditLogDetail = new AuditLogDetail()
							{
								AuditLogId = logId,
								EntityName = entry.Metadata.Name,
								PropertyName = property.Metadata.Name,
								NewValue = property.CurrentValue?.ToString(),
								OldValue = property.OriginalValue?.ToString()
							};
							AuditLogDetails.Add(auditLogDetail);
						}
					}
					else if (entry.State is EntityState.Deleted)
					{
						foreach (var p in entry.Properties)
						{
							var auditLogDetail = new AuditLogDetail()
							{
								AuditLogId = logId,
								EntityName = entry.Metadata.Name,
								PropertyName = p.Metadata.Name,
								OldValue = p.OriginalValue?.ToString()
							};
							AuditLogDetails.Add(auditLogDetail);
						}
					}
					else if (entry.State is EntityState.Added)
					{
						foreach (var p in entry.Properties)
						{
							var auditLogDetail = new AuditLogDetail()
							{
								AuditLogId = logId,
								EntityName = entry.Metadata.Name,
								PropertyName = p.Metadata.Name,
								NewValue = p.OriginalValue?.ToString()
							};
							AuditLogDetails.Add(auditLogDetail);
						}
					}
				}
			}
			await SaveChangesAsync();
		}

	}
}
