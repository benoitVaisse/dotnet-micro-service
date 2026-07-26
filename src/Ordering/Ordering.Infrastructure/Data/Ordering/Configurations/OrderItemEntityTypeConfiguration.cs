using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Domain.Models.Order;

namespace Ordering.Infrastructure.Data.Ordering.Configurations;

public class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id).HasColumnName("id");

        builder.Property(oi => oi.OrderId).HasColumnName("order_id");

        builder.Property(oi => oi.ProductId).HasColumnName("product_id");

        builder.Property(oi => oi.ProductName)
            .HasColumnName("product_name")
            .IsRequired().HasMaxLength(100);

        builder.Property(oi => oi.PictureUrl)
            .HasColumnName("picture_url")
            .IsRequired().HasMaxLength(500);

        builder.Property(oi => oi.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2);

        builder.Property(oi => oi.Discount)
            .HasColumnName("discount")
            .HasPrecision(18, 2);

        builder.Property(oi => oi.Units)
            .HasColumnName("units");
    }
}
