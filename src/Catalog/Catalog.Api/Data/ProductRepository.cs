using Catalog.Api.Models;
using Catalog.Api.Models.CRUD;
using Catalog.Api.Models.Filter;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Data;

public class ProductRepository(
        CatalogDbContext context
    ) : IProductRepository
{
    public async Task<IEnumerable<Product>> GetFiltered(FilterRequest filterRequest)
    {
        IQueryable<Product> query = context.Products.AsQueryable();
        query = query.FilterBy(filterRequest)
            .SortFilter(filterRequest)
            .Pagination(filterRequest);

        IEnumerable<Product> products = await query.ToListAsync();
        return products;
    }

    public async Task<Product?> Read(Guid id)
    {
        return await context.Products
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.Id == id);
    }
}
