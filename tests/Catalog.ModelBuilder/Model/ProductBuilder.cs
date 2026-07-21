using Catalog.Api.Models;
using Catalog.ModelBuilder.Shared;

namespace Catalog.ModelBuilder.Model;

/// <summary>
/// Builds <see cref="Product"/> instances for tests, pre-filled with valid defaults so a test
/// only has to state the properties it actually cares about.
/// </summary>
public class ProductBuilder : Builder<Product>
{
    public ProductBuilder()
    {
        _entity.Id = Guid.NewGuid();
        _entity.Name = "Test product";
        _entity.Description = "Test description";
        _entity.Price = 10.99m;
        _entity.AvailableStock = 10;
    }

    public ProductBuilder WithIdentifier(Guid identifier)
    {
        _entity.Id = identifier;
        return this;
    }

    public ProductBuilder WithName(string name)
    {
        _entity.Name = name;
        return this;
    }

    public ProductBuilder WithDescription(string? description)
    {
        _entity.Description = description;
        return this;
    }

    public ProductBuilder WithPrice(decimal price)
    {
        _entity.Price = price;
        return this;
    }

    public ProductBuilder WithAvailableStock(int availableStock)
    {
        _entity.AvailableStock = availableStock;
        return this;
    }
}
