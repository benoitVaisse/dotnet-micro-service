using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Models.Client;
using Ordering.Domain.Models.Order;
using Ordering.Infrastructure.Data.Client.Configurations;
using Ordering.Infrastructure.Data.Ordering.Configurations;

namespace Ordering.Infrastructure;

public class OrderingDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Buyer> Buyers => Set<Buyer>();

    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ordering");
        modelBuilder.ApplyConfiguration(new OrderEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new BuyerEntityTypeConfiguration());

        //modelBuilder.UseIntegrationEventLogs();
    }
}
