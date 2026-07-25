# Domain event dispatch — interceptors

This folder contains the EF Core interceptor that dispatches domain events raised by aggregates.

The key design decision here is **when** events are dispatched relative to the database
transaction: **pre-commit** or **post-commit**. This README documents that choice.

---

## How it works

`DomaineEventInterceptor` hooks into EF Core's save pipeline. On each save it:

1. Reads the tracked `AggregateEntity` instances from the `ChangeTracker`.
2. Collects their domain events and clears them from the entities.
3. Hands the events to `IDomainEventDispatcher`, which resolves and invokes the matching
   `IDomainEventHandler<T>` handlers.

The interceptor is attached to the `DbContext` via `.AddInterceptors(...)`.

---

## The decision: post-commit dispatch

Events are dispatched from **`SavedChangesAsync`** — i.e. **after** `SaveChanges` has completed
and the transaction is committed.

This is a deliberate choice, not a default. The two options differ in one fundamental way: the
**transaction boundary**.

### Post-commit (`SavedChangesAsync`) — current choice

Handlers run **after** the transaction is committed, so **outside** of it.

- ✅ The aggregate is already safely persisted before any handler runs.
- ✅ A slow or failing handler cannot roll back or slow down the save.
- ⚠️ If a handler fails, its effect is lost while the aggregate stays saved — the two are **not
  atomic**.

**Suited for:** non-critical, decoupled side effects — sending an email, writing a log,
publishing an external notification. Losing one of these on failure is acceptable.

### Pre-commit (`SavingChangesAsync`) — the alternative

Handlers run **before** the save completes, **inside** the same transaction.

- ✅ Handlers are **atomic** with the save: if a handler throws, everything rolls back, keeping
  data consistent.
- ⚠️ A slow handler slows down the transaction; a handler doing external I/O can make the whole
  save fail.

**Suited for:** side effects that must succeed or fail together with the aggregate — e.g. writing
another aggregate in the same unit of work.

---

## How to switch

The dispatcher can be wired to either hook (or both). Override:

- `SavingChangesAsync` → dispatch **before** the commit (pre-commit).
- `SavedChangesAsync` → dispatch **after** the commit (post-commit, current).

The choice is driven by the business need: whether a given side effect must be atomic with the
save or can run independently afterwards.

---

## Going further: the Outbox pattern

Post-commit dispatch raises a known question: *what if a handler fails after the aggregate is
already committed?* The event is then lost.

The robust answer is the **Outbox pattern**: the event is persisted **in the same transaction**
as the aggregate (so it is never lost), and a separate process reads the outbox and publishes the
event reliably afterwards. This decouples "recording that something happened" (atomic with the
save) from "reacting to it" (done reliably, out of band).

Not implemented here — noted as the natural evolution when reliable delivery becomes a
requirement.