using Catalog.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationsTests.Infrastructure;

public abstract class IntegrationTestBase
{
    protected CatalogDbContext Context { get; private set; } = null!;

    [SetUp]
    public async Task SetUpDatabaseAsync()
    {
        Context = DatabaseFixture.CreateContext();

        await Context.Database.ExecuteSqlRawAsync(
            """TRUNCATE TABLE "Products";""");
    }

    [TearDown]
    public async Task TearDownDatabaseAsync()
    {
        await Context.DisposeAsync();
    }
}
