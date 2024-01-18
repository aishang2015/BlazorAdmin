using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Data;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities;
using BlazorAdmin.Data.Entities.Chat;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BlazorAdmin.Im.Backgrounds
{
    public class SendMessageBackgroundService : BackgroundService
    {
        private readonly ILogger<SendMessageBackgroundService> _logger;

        private readonly IServiceProvider _serviceProvider;

        private readonly BlazroAdminChatDbContextFactory _messageDbContextFactory;

        public SendMessageBackgroundService(ILogger<SendMessageBackgroundService> logger,
            IServiceProvider serviceProvider, BlazroAdminChatDbContextFactory dbContextFactory)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _messageDbContextFactory = dbContextFactory;
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

                    using var trans = mainContext.Database.BeginTransaction();

                    BlazroAdminChatDbContext messageDbContext;
                    int channelId;

                    // 特殊用户发送
                    if (message.ReceiverId != null)
                    {
                        var systemChannel = (from channel in mainContext.ChatChannels
                                             join cm1 in mainContext.ChatChannelMembers on channel.Id equals cm1.ChatChannelId
                                             join cm2 in mainContext.ChatChannelMembers on channel.Id equals cm2.ChatChannelId
                                             where cm1.MemberId == message.SenderId &&
                                                 cm2.MemberId == message.ReceiverId
                                             select channel).FirstOrDefault();

                        if (systemChannel == null)
                        {

                            // 频道表
                            var channel = mainContext.ChatChannels.Add(new ChatChannel { Type = (int)ChatChannelType.系统对话 });
                            await mainContext.SaveChangesAsync();

                            // 成员表
                            mainContext.ChatChannelMembers.AddRange([
                                new ChatChannelMember { ChatChannelId = channel.Entity.Id, MemberId = message.ReceiverId.Value },
                                new ChatChannelMember { ChatChannelId = channel.Entity.Id, MemberId = message.SenderId },
                            ]);
                            await mainContext.SaveChangesAsync();

                            channelId = channel.Entity.Id;
                            messageDbContext = _messageDbContextFactory.CreateDbContext(channel.Entity.Id);
                            messageDbContext.Database.EnsureCreated();
                        }
                        else
                        {
                            channelId = systemChannel.Id;
                            messageDbContext = _messageDbContextFactory.CreateDbContext(systemChannel.Id);
                        }
                    }
                    else
                    {
                        if (message.ChannelId != null)
                        {
                            channelId = message.ChannelId.Value;
                            messageDbContext = _messageDbContextFactory.CreateDbContext(message.ChannelId.Value);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                    // 保存未读数量
                    var msgNoReads = mainContext.ChatChannelMembers
                        .Where(m => m.ChatChannelId == channelId)
                        .ToList();
                    foreach (var msgNoRead in msgNoReads)
                    {
                        msgNoRead.NotReadCount = msgNoRead.NotReadCount + 1;
                    }
                    await mainContext.SaveChangesAsync();
                    trans.Commit();

                    // 保存聊天消息
                    messageDbContext.ChatMessages.Add(new ChatMessage
                    {
                        CreatedTime = DateTime.Now,
                        Content = message.Content,
                        SenderId = message.SenderId,
                        Type = 1
                    });
                    await messageDbContext.SaveChangesAsync();

                    // 在线用户推送
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub, IChatClient>>();
                    var onlineClients = ChatHub.OnlineUsers
                        .Where(kv => msgNoReads.Any(r => r.MemberId == kv.Key))
                        .Select(kv => kv.Value);
                    await hubContext.Clients.Clients(onlineClients).ReceiveMessage(new ChatMessageReceivedModel
                    {
                        ChannelId = channelId,
                        SenderId = message.SenderId,
                        Content = message.Content,
                    });
                    messageDbContext.Dispose();
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
        }
    }
}
