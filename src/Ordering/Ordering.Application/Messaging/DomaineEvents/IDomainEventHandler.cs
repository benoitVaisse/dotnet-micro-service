using Ordering.Domain.Shared;

namespace Ordering.Application.Messaging.DomaineEvents;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task HandleAsync(T domaineEvent, CancellationToken cancellationToken = default);
}
