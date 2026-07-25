using Ordering.Domain.Shared;

namespace Ordering.Domain.Models.Order;

public class Order : IAggregateRoot
{
    public Guid Id { get; set; }

    public OrderStatus OrderStatus { get; private set; }

    public string? Description { get; private set; }
}
