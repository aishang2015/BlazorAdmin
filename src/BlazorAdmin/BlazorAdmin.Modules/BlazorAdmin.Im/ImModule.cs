﻿using BlazorAdmin.Im.Backgrounds;
using BlazorAdmin.Im.Core;
using BlazorAdmin.Im.Events;
using BlazorAdmin.Servers.Core.Helper;
using BlazorAdmin.Servers.Core.Modules;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorAdmin.Im
{
    public class ImModule : IModule
    {
        public IServiceCollection Add(IServiceCollection services)
        {
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
