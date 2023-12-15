using BlazorAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Data
{
    public class BlazroAdminChatDbContext : DbContext
    {
        public BlazroAdminChatDbContext(DbContextOptions<BlazroAdminChatDbContext> options) : base(options)
        { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
