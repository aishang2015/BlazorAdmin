using BlazorAdmin.Layout.States;
using BlazorAdmin.Servers.Core.Modules;
using BlazorAdmin.Servers.Core.States;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin.Layout
{
    public class LayoutModule : IModule
    {
        public IServiceCollection Add(IServiceCollection services)
        {
            services.AddScoped<ThemeState>();
            services.AddScoped<LayoutState>();
            return services;
        }
    }
}
