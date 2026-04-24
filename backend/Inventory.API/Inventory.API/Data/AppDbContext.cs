using Microsoft.EntityFrameworkCore;
using Inventory.API.Models;

namespace Inventory.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<IntegrationEventOutbox> IntegrationEventOutboxes { get; set; }
    }
}