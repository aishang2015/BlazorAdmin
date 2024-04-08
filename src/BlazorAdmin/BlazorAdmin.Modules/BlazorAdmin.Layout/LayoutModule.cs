using BlazorAdmin.Core.Modules;
using BlazorAdmin.Layout.States;
using Microsoft.AspNetCore.Builder;
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
