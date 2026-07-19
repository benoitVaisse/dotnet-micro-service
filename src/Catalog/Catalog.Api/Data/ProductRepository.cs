using Catalog.Api.Models;
using Catalog.Api.Models.CRUD;
using Catalog.Api.Models.Filter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Catalog.Api.Data;

public class ProductRepository(
        CatalogDbContext context
    ) : IProductRepository
{
    public async Task<Product> Add(Product product)
    {
        EntityEntry<Product> productCreated = await context.AddAsync(product);
        return productCreated.Entity;
    }

    public void Delete(Product product)
    {
        EntityEntry<Product> productRemoved = context.Remove(product);
    }

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

    public async Task<Product?> ReadTracked(Guid id)
    {
        return await context.Products.FindAsync(id);
    }

    public async Task SaveChangeAsync()
    {
        await context.SaveChangesAsync();
    }
}
