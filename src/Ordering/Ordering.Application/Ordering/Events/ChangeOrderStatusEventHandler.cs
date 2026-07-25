using Ordering.Application.Messaging.DomaineEvents;
using Ordering.Domain.Models.Order.Events;

namespace Ordering.Application.Ordering.Events;

public class ChangeOrderStatusEventHandler() : IDomainEventHandler<ChangeOrderStatusEvent>
{
    public async Task HandleAsync(ChangeOrderStatusEvent domaineEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"ChangeOrderStatusEventHandler on {domaineEvent.OrderId}");
        await Task.CompletedTask;
    }
}
