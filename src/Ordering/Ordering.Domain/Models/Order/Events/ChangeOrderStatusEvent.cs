using Ordering.Domain.Shared;

namespace Ordering.Domain.Models.Order.Events;

public record ChangeOrderStatusEvent(Guid OrderId) : IDomainEvent;
