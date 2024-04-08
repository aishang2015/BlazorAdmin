using BlazorAdmin.Core.Chat;
using BlazorAdmin.Core.Helper;
using BlazorAdmin.Core.Modules;
using BlazorAdmin.Im.Backgrounds;
using BlazorAdmin.Im.Core;
using BlazorAdmin.Im.Data;
using BlazorAdmin.Im.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin.Im
{
    public class ImModule : IModule
    {
        public IServiceCollection Add(IServiceCollection services)
        {
            services.AddSingleton<BlazroAdminChatDbContextFactory>();
            services.AddSingleton<MessageSender>();
            services.AddHostedService<SendMessageBackgroundService>();
            services.AddScoped<EventHelper<UpdateNoReadCountEvent>>();

            return services;
        }

        public WebApplication Use(WebApplication app)
        {
            app.MapHub<ChatHub>(ChatHub.ChatHubUrl);
            return app;
        }
    }
}
