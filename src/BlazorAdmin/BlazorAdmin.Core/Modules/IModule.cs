using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Modules
{
    public interface IModule
    {
        public IServiceCollection Add(IServiceCollection services) => services;

        public WebApplication Use(WebApplication app) => app;
    }
}
