using Ordering.Domain.Shared;
using System.ComponentModel.DataAnnotations;

namespace Ordering.Domain.Models.Order;

public class OrderItem : Entity
{
    [Required]
    public string ProductName { get; private set; }

    public string? PictureUrl { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal Discount { get; private set; }

    public int Units { get; private set; }

    public int ProductId { get; private set; }

    protected OrderItem() { }

    public static Result<OrderItem> Create(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        if (units <= 0)
        {
            return Result<OrderItem>.Failure("Invalid number of units");
        }

        if ((unitPrice * units) < discount)
        {
            return Result<OrderItem>.Failure("The total of order item is lower than applied discount");
        }
        OrderItem orderItem = new()
        {

            ProductId = productId,

            ProductName = productName,
            UnitPrice = unitPrice,
            Discount = discount,
            Units = units,
            PictureUrl = pictureUrl,
        };

        return Result<OrderItem>.Success(orderItem);
    }
}
