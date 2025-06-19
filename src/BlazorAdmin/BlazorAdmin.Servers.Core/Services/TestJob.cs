using Microsoft.Extensions.Logging;
using Quartz;

namespace BlazorAdmin.Servers.Core.Services
{
    public class TestJob : IJob
    {
        private readonly ILogger _logger;

        public TestJob(ILogger<TestJob> logger)
        {
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("TestJob executed at {0}", DateTime.Now);
        }
    }
}
