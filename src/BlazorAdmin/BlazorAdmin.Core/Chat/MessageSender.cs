using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace BlazorAdmin.Core.Chat
{
    public class MessageSender
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly ConcurrentDictionary<string, int> _specialUserDic = new();

        public MessageSender(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<bool> SendSystemMessage(int targetUserId, string content)
        {
            var senderId = await GetUserId("SystemNotification");
            if (senderId == null)
            {
                return false;
            }

            await ChannelHelper<ChatMessageSendModel>.Instance.Writer.WriteAsync(new ChatMessageSendModel
            {
                SenderId = senderId.Value,
                Content = content,
                ReceiverId = targetUserId
            });
            return true;
        }

        public async Task<bool> SendChannelMessage(int senderId, int targetChannel, string content)
        {
            await ChannelHelper<ChatMessageSendModel>.Instance.Writer.WriteAsync(new ChatMessageSendModel
            {
                SenderId = senderId,
                Content = content,
                ChannelId = targetChannel
            });
            return true;
        }

        private async Task<int?> GetUserId(string name)
        {
            if (!_specialUserDic.TryGetValue(name, out var sourceId))
            {
                using var scope = _serviceProvider.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlazorAdminDbContext>>();
                using var context = await factory.CreateDbContextAsync();

                var user = context.Users.FirstOrDefault(u => u.Name == name);
                return user?.Id;
            }

            return sourceId;
        }
    }
}
