using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Models.Order;

namespace Ordering.Infrastructure.Data.Ordering.Configurations;

public class OrderingConfigurationEntityType : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.Ignore(b => b.DomainEvents);

        builder.Property(o => o.Id)
            .UseHiLo("orderseq");

        //Address value object persisted as owned entity type supported since EF Core 2.0
        builder
            .OwnsOne(o => o.Address);

        builder
            .Property(o => o.OrderStatus)
            .HasConversion<string>()
            .HasMaxLength(30);
    }
}
