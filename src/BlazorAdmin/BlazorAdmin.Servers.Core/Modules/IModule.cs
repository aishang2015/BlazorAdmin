using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin.Core.Modules
{
    public interface IModule
    {
        public IServiceCollection Add(IServiceCollection services) => services;

        public WebApplication Use(WebApplication app) => app;
    }
}
