using BlazorAdmin.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Data
{
    public class BlazroAdminChatDbContextFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        private readonly IConfiguration _configuration;

        public BlazroAdminChatDbContextFactory(ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            _loggerFactory = loggerFactory;
            _configuration = configuration;
        }

        public BlazroAdminChatDbContext CreateDbContext(int channelId)
        {
            var connection = string.Format(_configuration.GetConnectionString("ChatMessageTpl")!, channelId);

            var optionsBuilder = new DbContextOptionsBuilder<BlazroAdminChatDbContext>()
                .UseLoggerFactory(_loggerFactory)
                .UseSqlite(connection)
                .EnableSensitiveDataLogging();
            return new BlazroAdminChatDbContext(optionsBuilder.Options);
        }
    }
}
