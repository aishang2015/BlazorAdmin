using BlazorAdmin.Core.Constants;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Security.Cryptography;
using Role = BlazorAdmin.Data.Entities.Role;
using User = BlazorAdmin.Data.Entities.User;

namespace BlazorAdmin.Core.Data
{
	public static class BlazorAdminDbContextExtension
	{
		public static void InitialData(this BlazorAdminDbContext dbContext)
		{
			if (dbContext.Database.EnsureCreated())
			{
				var rsa = RSA.Create();
				var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
				var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

				using var tran = dbContext.Database.BeginTransaction();

				dbContext.Settings.AddRange(new Setting
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
				dbContext.SaveChanges();

				dbContext.Menus.Add(new Menu { Name = "首页", Type = 1, Route = "/", Order = 1, Icon = Icons.Material.Filled.Home });
				var entry = dbContext.Menus.Add(new Menu { Name = "日志", Type = 1, Route = "/", Order = 2, Icon = Icons.Material.Filled.Info });
				var entry2 = dbContext.Menus.Add(new Menu { Name = "权限", Type = 1, Route = "/", Order = 3, Icon = Icons.Material.Filled.VerifiedUser });
				var entry3 = dbContext.Menus.Add(new Menu { Name = "系统", Type = 1, Route = "/", Order = 4, Icon = Icons.Material.Filled.Computer });
				dbContext.Menus.Add(new Menu { Name = "关于", Type = 1, Route = "/about", Order = 4, Icon = Icons.Material.Filled.TextFields });
				dbContext.SaveChanges();

				dbContext.Menus.Add(new Menu { ParentId = entry.Entity.Id, Name = "审计", Type = 1, Route = "/auditLog", Order = 1, Icon = Icons.Material.Filled.Verified });
				dbContext.Menus.Add(new Menu { ParentId = entry.Entity.Id, Name = "登录", Type = 1, Route = "/loginLog", Order = 2, Icon = Icons.Material.Filled.Login });
				dbContext.SaveChanges();

				var userManageEnty = dbContext.Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "用户", Type = 1, Route = "/user", Order = 1, Icon = Icons.Material.Filled.Person });
				var roleManageEntry = dbContext.Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "角色", Type = 1, Route = "/role", Order = 2, Icon = Icons.Material.Filled.LockPerson });
				var menuManageEntry = dbContext.Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "菜单", Type = 1, Route = "/menu", Order = 3, Icon = Icons.Material.Filled.Menu });
				dbContext.SaveChanges();

				dbContext.Menus.Add(new Menu { ParentId = entry3.Entity.Id, Name = "配置", Type = 1, Route = "/setting", Order = 1, Icon = Icons.Material.Filled.Settings });
				dbContext.SaveChanges();

				dbContext.Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "添加按钮", Type = 2, Order = 1, Identify = "UserAddBtn" });
				dbContext.Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "修改按钮", Type = 2, Order = 2, Identify = "UserUpdateBtn" });
				dbContext.Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "删除按钮", Type = 2, Order = 3, Identify = "UserDeleteBtn" });
				dbContext.Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "修改密码按钮", Type = 2, Order = 4, Identify = "UserPwdChangeBtn" });
				dbContext.Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "设置角色", Type = 2, Order = 5, Identify = "UserRoleSettingBtn" });
				dbContext.Menus.Add(new Menu { ParentId = userManageEnty.Entity.Id, Name = "启用选框", Type = 2, Order = 6, Identify = "UserActiveBtn" });

				dbContext.Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "添加按钮", Type = 2, Order = 1, Identify = "RoleAddBtn" });
				dbContext.Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "修改按钮", Type = 2, Order = 2, Identify = "RoleUpdateBtn" });
				dbContext.Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "删除按钮", Type = 2, Order = 3, Identify = "RoleDeleteBtn" });
				dbContext.Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "菜单权限按钮", Type = 2, Order = 3, Identify = "RoleMenuBtn" });
				dbContext.Menus.Add(new Menu { ParentId = roleManageEntry.Entity.Id, Name = "启用选框", Type = 2, Order = 6, Identify = "RoleActiveBtn" });

				dbContext.Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "添加按钮", Type = 2, Order = 1, Identify = "MenuAddBtn" });
				dbContext.Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "修改按钮", Type = 2, Order = 2, Identify = "MenuUpdateBtn" });
				dbContext.Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "删除按钮", Type = 2, Order = 3, Identify = "MenuDeleteBtn" });
				dbContext.Menus.Add(new Menu { ParentId = menuManageEntry.Entity.Id, Name = "排序按钮", Type = 2, Order = 4, Identify = "MenuOrderBtn" });


				var roleEntry = dbContext.Roles.Add(new Role { IsEnabled = true, Name = "Admin" });
				dbContext.SaveChanges();

				var userEntry = dbContext.Users.Add(new User
				{
					Name = "BlazorAdmin",
					IsEnabled = true,
					PasswordHash = HashHelper.HashPassword("BlazorAdmin"),
					RealName = "BlazorAdmin"
				});
				dbContext.SaveChanges();

				dbContext.UserRoles.Add(new UserRole
				{
					UserId = userEntry.Entity.Id,
					RoleId = roleEntry.Entity.Id
				});
				dbContext.SaveChanges();

				dbContext.RoleMenus.AddRange(dbContext.Menus.Select(m => m.Id).Select(id => new RoleMenu
				{
					MenuId = id,
					RoleId = roleEntry.Entity.Id
				}));
				dbContext.SaveChanges();

				tran.Commit();
			}
		}

		public static async Task CustomSaveChangesAsync(this BlazorAdminDbContext dbContext,
			AuthenticationStateProvider provider)
		{
			var userState = await provider.GetAuthenticationStateAsync();
			if (userState.User.Identity != null && userState.User.Identity.IsAuthenticated)
			{
				var userId = userState.User.GetUserId();
				var entries = dbContext.ChangeTracker.Entries()
					.Where(e => e.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
					.ToList();

				var transactionId = dbContext.Database.CurrentTransaction?.TransactionId ?? Guid.NewGuid();

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
					dbContext.AuditLogs.Add(auditLog);


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
							dbContext.AuditLogDetails.Add(auditLogDetail);
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
							dbContext.AuditLogDetails.Add(auditLogDetail);
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
							dbContext.AuditLogDetails.Add(auditLogDetail);
						}
					}
				}
			}
			await dbContext.SaveChangesAsync();
		}
	}
}
