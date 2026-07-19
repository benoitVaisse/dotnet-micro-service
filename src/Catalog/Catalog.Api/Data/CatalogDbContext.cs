using Catalog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Data;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Always call the base method to ensure any configurations from the base class are applied.
        base.OnModelCreating(builder);

        // Automatically apply all configurations defined in the current assembly.
        // This is particularly useful for large applications, as it eliminates the need to
        // manually register each configuration class in the 'OnModelCreating' method.
        builder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);
    }
}
