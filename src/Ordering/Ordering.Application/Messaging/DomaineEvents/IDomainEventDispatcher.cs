using Ordering.Domain.Shared;

namespace Ordering.Application.Messaging.DomaineEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
