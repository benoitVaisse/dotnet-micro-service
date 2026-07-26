using Ordering.Domain.Models.Client;
using Ordering.Domain.Shared;

namespace Ordering.Domain.Models.Order;

public class Order : AggregateEntity
{
    public DateTime OrderDate { get; private set; }

    public OrderStatus OrderStatus { get; private set; }

    public string? Description { get; private set; }

    public Address Address { get; private set; }

    public Guid? BuyerId { get; private set; }

    public Buyer Buyer { get; }

    // DDD Patterns comment
    // Using a private collection field, better for DDD Aggregate's encapsulation
    // so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
    // but only through the method OrderAggregateRoot.AddOrderItem() which includes behavior.
    private readonly List<OrderItem> _orderItems = [];

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public static Result<Order> Create(string description, Address address, Guid? buyerId = null)
    {
        // ici les invariants de création : au moins un item, etc.
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderStatus = OrderStatus.Submitted,
            Description = description,
            OrderDate = DateTime.UtcNow,
            Address = address,
            BuyerId = buyerId
        };
        return Result<Order>.Success(order);
    }

    public Order SetDescription(string description)
    {
        Description = description;
        return this;
    }

    public Order SetStatus(OrderStatus orderStatus)
    {
        OrderStatus = orderStatus;
        return this;
    }

    public Order SetOrderDate(DateTime orderDate)
    {
        OrderDate = orderDate;
        return this;
    }

    // DDD Patterns comment
    // This Order AggregateRoot's method "AddOrderItem()" should be the only way to add Items to the Order,
    // so any behavior (discounts, etc.) and validations are controlled by the AggregateRoot 
    // in order to maintain consistency between the whole Aggregate. 
    public Result AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount, string pictureUrl, int units = 1)
    {
        //add validated new order item
        Result<OrderItem> orderItemResult = OrderItem.Create(productId, productName, unitPrice, discount, pictureUrl, units);
        if (!orderItemResult.IsSuccess)
            return Result.Failure(orderItemResult.ErrorMessage!);

        _orderItems.Add(orderItemResult.Value!);
        return Result.Success();
    }
}
