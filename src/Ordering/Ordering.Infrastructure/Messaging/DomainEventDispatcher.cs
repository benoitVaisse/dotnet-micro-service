using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Messaging.DomaineEvents;
using Ordering.Domain.Shared;
using System.Reflection;

namespace Ordering.Infrastructure.Messaging;

public class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domaineEvent in domainEvents)
        {
            using var scope = serviceProvider.CreateScope();
            Type handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domaineEvent.GetType());

            IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);
            foreach (object? handler in handlers)
            {
                if (handler is null)
                    continue;

                MethodInfo? handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
                if (handleMethod is not null)
                {
                    await (Task)handleMethod.Invoke(handler, [domaineEvent, cancellationToken])!;
                }
            }

        }
    }
}
