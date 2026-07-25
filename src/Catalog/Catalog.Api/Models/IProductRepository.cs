using Catalog.Api.Models.CRUD;
using Catalog.Api.Models.Filter;

namespace Catalog.Api.Models;

/// <summary>
/// product repository
/// </summary>
public interface IProductRepository : IRead<Product>
{
    Task<IEnumerable<Product>> GetFiltered(FilterRequest filterRequest);

    Task<Product> Add(Product product);

    void Delete(Product product);

    Task<Product?> ReadTracked(Guid id);

    Task SaveChangeAsync();
}
