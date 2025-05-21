﻿using BlazorAdmin.Data.Constants;
using BlazorAdmin.Data.Entities.Ai;
using BlazorAdmin.Data.Entities.Chat;
using BlazorAdmin.Data.Entities.Log;
using BlazorAdmin.Data.Entities.Rbac;
using BlazorAdmin.Data.Entities.Setting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin.Data
{
    public class BlazorAdminDbContext : DbContext
    {
        private IServiceProvider _provider;

        public BlazorAdminDbContext(
            DbContextOptions<BlazorAdminDbContext> options,
             IServiceProvider provider) : base(options)
        {
            _provider = provider;
        }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<UserSetting> UserSettings { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<Menu> Menus { get; set; }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<OrganizationUser> OrganizationUsers { get; set; }

        public DbSet<RoleMenu> RoleMenus { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<AuditLogDetail> AuditLogDetails { get; set; }

        public DbSet<LoginLog> LoginLogs { get; set; }

        #region chat

        public DbSet<Group> Groups { get; set; }

        public DbSet<GroupMember> GroupMembers { get; set; }

        public DbSet<GroupMessage> GroupMessages { get; set; }

        public DbSet<GroupSetting> GroupSettings { get; set; }

        public DbSet<PrivateMessage> PrivateMessages { get; set; }

        public DbSet<PrivateSetting> PrivateSettings { get; set; }

        public DbSet<NotReadedMessage> NotReadedMessages { get; set; }

        #endregion

        #region Ai

        public DbSet<AiConfig> AiConfigs { get; set; }

        public DbSet<AiPrompt> AiPrompts { get; set; }

        public DbSet<AiRequestRecord> AiRequestRecords { get; set; }

        #endregion

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var pppp = _provider.GetRequiredService<AuthenticationStateProvider>();
            var user = await pppp.GetAuthenticationStateAsync();

            if (user.User.Identity != null && user.User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(user.User.Claims.FirstOrDefault(c => c.Type == ClaimConstant.UserId)!.Value);
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
                        OperateTime = DateTime.Now,
                        EntityName = entry.Metadata.Name,
                        Operation = (int)entry.State,
                        UserId = userId
                    };
                    AuditLogs.Add(auditLog);


                    if (entry.State is EntityState.Modified)
                    {
                        var modifiedProperties = entry.Properties.Where(p => p.IsModified ||
                            p.Metadata.Name == "Id");
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
                                NewValue = p.Metadata.Name == "Id" ? null : p.OriginalValue?.ToString()
                            };
                            AuditLogDetails.Add(auditLogDetail);
                        }
                    }
                }
            }
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}
