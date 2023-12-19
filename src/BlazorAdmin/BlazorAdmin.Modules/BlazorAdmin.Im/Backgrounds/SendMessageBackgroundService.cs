using BlazorAdmin.Core.Data;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Data;
using BlazorAdmin.Data.Entities;
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
                    var message = await ChannelHelper<ChatMessageModel>.Instance.Reader.ReadAsync(stoppingToken);

                    using var scope = _serviceProvider.CreateScope();
                    var _dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<BlazorAdminDbContext>>();
                    using var mainContext = _dbFactory.CreateDbContext();

                    BlazroAdminChatDbContext messageDbContext;

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
                            using var trans = mainContext.Database.BeginTransaction();

                            // 频道
                            var channel = mainContext.ChatChannels.Add(new ChatChannel { Type = (int)ChatChannelType.系统对话 });
                            await mainContext.SaveChangesAsync();

                            // 成员
                            mainContext.ChatChannelMembers.AddRange([
                                new ChatChannelMember { ChatChannelId = channel.Entity.Id, MemberId = message.ReceiverId.Value },
                                new ChatChannelMember { ChatChannelId = channel.Entity.Id, MemberId = message.SenderId },
                            ]);
                            await mainContext.SaveChangesAsync();
                            trans.Commit();

                            messageDbContext = _messageDbContextFactory.CreateDbContext(channel.Entity.Id);
                            messageDbContext.Database.EnsureCreated();
                        }
                        else
                        {
                            messageDbContext = _messageDbContextFactory.CreateDbContext(systemChannel.Id);
                        }
                    }
                    else
                    {
                        if (message.ChannelId != null)
                        {
                            messageDbContext = _messageDbContextFactory.CreateDbContext(message.ChannelId.Value);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }

                    messageDbContext.ChatMessages.Add(new ChatMessage
                    {
                        CreatedTime = DateTime.Now,
                        Content = message.Content,
                        SenderId = message.SenderId,
                        Type = 1
                    });
                    await messageDbContext.SaveChangesAsync();
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
