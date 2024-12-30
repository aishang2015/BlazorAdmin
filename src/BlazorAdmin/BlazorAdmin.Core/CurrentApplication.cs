using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core
{
    public static class CurrentApplication
    {
        public static WebApplication Application { get; set; } = null!;
    }
}
