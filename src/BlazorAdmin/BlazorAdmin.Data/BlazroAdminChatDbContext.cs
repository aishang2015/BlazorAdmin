using BlazorAdmin.Data.Entities.Chat;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data
{
    public class BlazroAdminChatDbContext : DbContext
    {
        public BlazroAdminChatDbContext(DbContextOptions<BlazroAdminChatDbContext> options) : base(options)
        { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
    }
}
