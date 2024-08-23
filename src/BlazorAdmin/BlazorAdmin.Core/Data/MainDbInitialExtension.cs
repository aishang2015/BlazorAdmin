using BlazorAdmin.Data.Constants;
using BlazorAdmin.Data.Entities.Rbac;
using BlazorAdmin.Data.Entities.Setting;
using BlazorAdmin.Data;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BlazorAdmin.Core.Helper;

namespace BlazorAdmin.Core.Data
{
    public static class MainDbInitialExtension
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
                var organizationManageEntry = dbContext.Menus.Add(new Menu { ParentId = entry2.Entity.Id, Name = "组织", Type = 1, Route = "/organization", Order = 4, Icon = Icons.Material.Filled.AccountTree });
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

                dbContext.Menus.Add(new Menu { ParentId = organizationManageEntry.Entity.Id, Name = "添加按钮", Type = 2, Order = 1, Identify = "OrganizationAddBtn" });
                dbContext.Menus.Add(new Menu { ParentId = organizationManageEntry.Entity.Id, Name = "修改按钮", Type = 2, Order = 2, Identify = "OrganizationUpdateBtn" });
                dbContext.Menus.Add(new Menu { ParentId = organizationManageEntry.Entity.Id, Name = "删除按钮", Type = 2, Order = 3, Identify = "OrganizationDeleteBtn" });
                dbContext.Menus.Add(new Menu { ParentId = organizationManageEntry.Entity.Id, Name = "排序按钮", Type = 2, Order = 4, Identify = "OrganizationOrderBtn" });

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
    }
}
