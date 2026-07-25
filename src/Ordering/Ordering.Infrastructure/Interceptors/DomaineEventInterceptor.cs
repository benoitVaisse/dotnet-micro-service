using Microsoft.EntityFrameworkCore.Diagnostics;
using Ordering.Application.Messaging.DomaineEvents;
using Ordering.Domain.Shared;

namespace Ordering.Infrastructure.Interceptors;

/// <summary>
/// Collects domain events from the <see cref="AggregateEntity"/> instances tracked by EF Core
/// and dispatches them AFTER the save completes (post-commit, via <c>SavedChangesAsync</c>).
/// </summary>
/// <remarks>
/// Post-commit is a deliberate choice: handlers run once the transaction is already committed,
/// hence outside of it. This suits non-critical, decoupled side effects (email, logging,
/// external events) — if a handler fails, the aggregate stays persisted. For side effects that
/// must be atomic with the save, dispatch in pre-commit (<c>SavingChangesAsync</c>) instead, or
/// use an Outbox pattern for reliable publishing. See the README in this folder for the rationale.
/// </remarks>
internal sealed class DomaineEventInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {

        IEnumerable<IDomainEvent> domaineEvents = [.. eventData.Context!.ChangeTracker
                                .Entries<AggregateEntity>()
                                .Select(entry => entry.Entity)
                                .SelectMany(entity =>
                                {
                                    List<IDomainEvent> domainEvents = [.. entity.DomainEvents];
                                    entity.ClearDomainEvents();
                                    return domainEvents;
                                })];

        await dispatcher.DispatchAsync(domaineEvents, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
}
