using Catalog.Api.Models;

namespace Catalog.ModelBuilder.Shared;

/// <summary>
/// Base class for test data builders: owns the entity being assembled and hands it over.
/// </summary>
/// <remarks>
/// Deliberately minimal - it declares no With… methods. Those belong on the concrete builder
/// so they can return the concrete type and keep the fluent chain usable:
/// <c>new ProductBuilder().WithName("x").WithPrice(2m).Build()</c>. Declaring them here would
/// force every method to return <typeparamref name="TEntity"/> (which breaks chaining after
/// the first call) or require a self-referencing generic, whose complexity is not worth it
/// for the one member actually shared: <see cref="Build"/>.
///
/// A builder instance is single-use: <see cref="Build"/> always returns the same object, so
/// creating several entities means creating several builders. That is what keeps each one
/// with its own generated identifier.
/// </remarks>
public abstract class Builder<TEntity> where TEntity : IEntity, new()
{
    protected readonly TEntity _entity = new();

    public TEntity Build() => _entity;
}
