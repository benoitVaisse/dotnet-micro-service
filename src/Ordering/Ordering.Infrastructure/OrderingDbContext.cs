using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Models.Order;

namespace Ordering.Infrastructure;

public class OrderingDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
}
