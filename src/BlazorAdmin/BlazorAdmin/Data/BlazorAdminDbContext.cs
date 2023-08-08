using BlazorAdmin.Data.Constants;
using BlazorAdmin.Data.Entities;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MudBlazor;
using System.Security.Cryptography;

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
				var rsa = RSA.Create();
				var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
				var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

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
					Key = JwtConstant.JwtSigningRsaPrivateKey,
					Value = privateKey
				}, new Setting
				{
					Key = JwtConstant.JwtSigningRsaPublicKey,
					Value = publicKey
				}, new Setting
				{
					Key = JwtConstant.JwtExpireMinute,
					Value = "720"
				});
				SaveChanges();

				Menus.Add(new Menu { Name = "首页", Type = 1, Route = "/", Order = 1, Icon = Icons.Material.Filled.Home });
				var entry = Menus.Add(new Menu { Name = "日志", Type = 1, Route = "/", Order = 2, Icon = Icons.Material.Filled.Info });
				var entry2 = Menus.Add(new Menu { Name = "权限", Type = 1, Route = "/", Order = 3, Icon = Icons.Material.Filled.VerifiedUser });
				Menus.Add(new Menu { Name = "关于", Type = 1, Route = "/about", Order = 4, Icon = Icons.Material.Filled.TextFields });
				SaveChanges();

				Menus.Add(new Menu { ParentId = entry.Entity.Id, Name = "审计", Type = 1, Route = "/auditLog", Order = 1, Icon = Icons.Material.Filled.Verified });
				Menus.Add(new Menu { ParentId = entry.Entity.Id, Name = "登录", Type = 1, Route = "/loginLog", Order = 2, Icon = Icons.Material.Filled.Login });
				SaveChanges();

				var userManageEnty = Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "用户", Type = 1, Route = "/user", Order = 1, Icon = Icons.Material.Filled.Person });
				var roleManageEntry = Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "角色", Type = 1, Route = "/role", Order = 2, Icon = Icons.Material.Filled.LockPerson });
				var menuManageEntry = Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "菜单", Type = 1, Route = "/menu", Order = 3, Icon = Icons.Material.Filled.Menu });
				SaveChanges();

				Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "添加按钮", Type = 2, Order = 1, Identify = "UserAddBtn" });
				Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "修改按钮", Type = 2, Order = 2, Identify = "UserUpdateBtn" });
				Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "删除按钮", Type = 2, Order = 3, Identify = "UserDeleteBtn" });
				Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "修改密码按钮", Type = 2, Order = 4, Identify = "UserPwdChangeBtn" });
				Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "设置角色", Type = 2, Order = 5, Identify = "UserRoleSettingBtn" });
				Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "启用选框", Type = 2, Order = 6, Identify = "UserActiveBtn" });

				Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "添加按钮", Type = 2, Order = 1, Identify = "RoleAddBtn" });
				Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "修改按钮", Type = 2, Order = 2, Identify = "RoleUpdateBtn" });
				Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "删除按钮", Type = 2, Order = 3, Identify = "RoleDeleteBtn" });
				Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "菜单权限按钮", Type = 2, Order = 3, Identify = "RoleMenuBtn" });
				Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "启用选框", Type = 2, Order = 6, Identify = "RoleActiveBtn" });

				Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "添加按钮", Type = 2, Order = 1, Identify = "MenuAddBtn" });
				Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "修改按钮", Type = 2, Order = 2, Identify = "MenuUpdateBtn" });
				Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "删除按钮", Type = 2, Order = 3, Identify = "MenuDeleteBtn" });
				Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "排序按钮", Type = 2, Order = 4, Identify = "MenuOrderBtn" });


				var roleEntry = Roles.Add(new Role { IsEnabled = true, Name = "Admin" });
				SaveChanges();

				var userEntry = Users.Add(new User
				{
					Name = "BlazorAdmin",
					IsEnabled = true,
					PasswordHash = HashHelper.HashPassword("BlazorAdmin"),
					RealName = "Administrator"
				});
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
