using BlazorAdmin.Servers.Core.Data;
using BlazorAdmin.Servers.Core.Data.Entities.Notification;
using BlazorAdmin.Servers.Core.Extension;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Servers.Core.Helper
{
    /// <summary>
    /// 通知帮助类
    /// </summary>
    public class NotificationHelper
    {
        private readonly IDbContextFactory<BlazorAdminDbContext> _dbContextFactory;

        private readonly AuthenticationStateProvider _stateProvider;

        public NotificationHelper(IDbContextFactory<BlazorAdminDbContext> dbContextFactory,
            AuthenticationStateProvider stateProvider)
        {
            _dbContextFactory = dbContextFactory;
            _stateProvider = stateProvider;
        }

        /// <summary>
        /// 发送系统通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="senderId">发送者Id</param>
        /// <param name="receiverIds">接收者Id列表</param>
        /// <returns></returns>
        public async Task SendSystemNotificationAsync(string title, string content, params int[] receiverIds)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    SendTime = DateTime.Now,
                    Type = 1, // 系统通知
                    Status = 1  // 已发送
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();

                var receivers = receiverIds.Select(receiverId => new NotificationReceiver
                {
                    NotificationId = notification.Id,
                    ReceiverId = receiverId,
                    IsRead = false
                }).ToList();

                context.NotificationReceivers.AddRange(receivers);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 发送个人通知
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="senderId">发送者Id</param>
        /// <param name="receiverId">接收者Id</param>
        /// <returns></returns>
        public async Task SendPersonalNotificationAsync(string title, string content, params int[] receiverIds)
        {
            var userState = await _stateProvider.GetAuthenticationStateAsync();
            using var context = await _dbContextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var notification = new Notification
                {
                    Title = title,
                    Content = content,
                    SenderId = userState.User.GetUserId(),
                    SendTime = DateTime.Now,
                    Type = 2, // 个人通知
                    Status = 1  // 已发送
                };

                context.Notifications.Add(notification);
                await context.SaveChangesAsync();

                var receivers = receiverIds.Select(receiverId => new NotificationReceiver
                {
                    NotificationId = notification.Id,
                    ReceiverId = receiverId,
                    IsRead = false
                }).ToList();

                context.NotificationReceivers.AddRange(receivers);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}