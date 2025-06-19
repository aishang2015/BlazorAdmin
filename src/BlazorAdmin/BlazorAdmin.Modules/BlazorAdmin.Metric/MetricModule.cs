using BlazorAdmin.Metric.Core;
using BlazorAdmin.Servers.Core.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin.Metric
{
    public class MetricModule : IModule
    {
        public IServiceCollection Add(IServiceCollection services)
        {
            services.AddSingleton(new MetricEventListener());
            return services;
        }
    }
}
