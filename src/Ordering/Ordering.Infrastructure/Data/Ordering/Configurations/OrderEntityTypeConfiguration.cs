using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Models.Order;

namespace Ordering.Infrastructure.Data.Ordering.Configurations;

public class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Ignore(b => b.DomainEvents);

        builder.Property(o => o.Id).HasColumnName("id");

        builder.Property(o => o.OrderDate).HasColumnName("order_date");

        builder.Property(o => o.Description).HasColumnName("description");

        builder.Property(o => o.BuyerId).HasColumnName("buyer_id");

        // Address value object mapped as an EF Core complex type (EF Core 8+)
        builder
            .ComplexProperty(o => o.Address, address =>
            {
                address.Property(o => o.Street).HasColumnName("address_street").IsRequired().HasMaxLength(200);
                address.Property(o => o.ZipCode).HasColumnName("address_zip_code").IsRequired().HasMaxLength(15);
                address.Property(o => o.City).HasColumnName("address_city").IsRequired().HasMaxLength(200);
                address.Property(o => o.Country).HasColumnName("address_country").IsRequired().HasMaxLength(200);
                address.Property(o => o.State).HasColumnName("address_state").HasMaxLength(200);
            });

        builder
            .Property(o => o.OrderStatus)
            .HasColumnName("order_status")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder
            .HasMany(o => o.OrderItems)
            .WithOne().HasForeignKey(oi => oi.OrderId);

        builder.Navigation(o => o.OrderItems)
            .HasField("_orderItems")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
