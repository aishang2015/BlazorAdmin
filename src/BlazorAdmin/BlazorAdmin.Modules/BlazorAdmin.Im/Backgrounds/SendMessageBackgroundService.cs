using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities.Chat;
using BlazorAdmin.Im.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Im.Backgrounds
{
    public class SendMessageBackgroundService : BackgroundService
    {
        private readonly ILogger<SendMessageBackgroundService> _logger;

        private readonly IServiceProvider _serviceProvider;

        public SendMessageBackgroundService(ILogger<SendMessageBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await ChannelHelper<ChatMessageSendModel>.Instance.Reader.ReadAsync(stoppingToken);

                    using var scope = _serviceProvider.CreateScope();
                    var _dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlazorAdminDbContext>>();
                    using var mainContext = _dbFactory.CreateDbContext();

                    using var trans = await mainContext.Database.BeginTransactionAsync(stoppingToken);

                    var memberList = new List<int>();
                    if (message.ChannelId != null)
                    {
                        var entry = mainContext.GroupMessages.Add(new GroupMessage
                        {
                            GroupId = message.ChannelId.Value,
                            Content = message.Content,
                            MessageType = message.MessageType,
                            SenderId = message.SenderId,
                            SendTime = DateTime.Now
                        });
                        memberList = mainContext.GroupMembers
                            .Where(gm => gm.GroupId == message.ChannelId.Value && gm.MemberId != message.SenderId)
                            .AsNoTracking().Select(m => m.MemberId).ToList();
                        mainContext.SaveChanges();

                        if (message.MessageType != 0)
                        {
                            foreach (var memberId in memberList)
                            {
                                mainContext.NotReadedMessages.Add(new NotReadedMessage
                                {
                                    UserId = memberId,
                                    GroupId = message.ChannelId.Value,
                                    MessageId = entry.Entity.Id,
                                });
                            }
                        }
                    }
                    else
                    {
                        var entry = mainContext.PrivateMessages.Add(new PrivateMessage
                        {
                            Content = message.Content,
                            MessageType = message.MessageType,
                            SenderId = message.SenderId,
                            ReceiverId = message.ReceiverId!.Value,
                            SendTime = DateTime.Now
                        });
                        memberList = new List<int> { message.ReceiverId!.Value, message.SenderId };
                        mainContext.SaveChanges();

                        if (message.MessageType != 0)
                        {
                            mainContext.NotReadedMessages.Add(new NotReadedMessage
                            {
                                UserId = message.ReceiverId.Value,
                                MessageId = entry.Entity.Id,
                                SendUserId = message.SenderId
                            });
                        }
                    }

                    mainContext.SaveChanges();
                    trans.Commit();

                    // 在线用户推送
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub, IChatClient>>();
                    var onlineClients = ChatHub.OnlineUsers
                        .Where(kv => memberList.Contains(kv.Key))
                        .Select(kv => kv.Value);
                    await hubContext.Clients.Clients(onlineClients).ReceiveMessage(new ChatMessageReceivedModel
                    {
                        ReceiverId = message.ReceiverId,
                        ChannelId = message.ChannelId,
                        SenderId = message.SenderId,
                        Content = message.Content,
                    });

                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
