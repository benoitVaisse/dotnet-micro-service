using Catalog.Api.Models.CRUD;
using Catalog.Api.Models.Filter;

namespace Catalog.Api.Models;

public interface IProductRepository : IRead<Product>
{
    Task<IEnumerable<Product>> GetFiltered(FilterRequest filterRequest);
}
