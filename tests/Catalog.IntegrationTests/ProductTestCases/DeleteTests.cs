using Catalog.Api.Data;
using Catalog.Api.Models;
using Catalog.IntegrationTests.Infrastructure;
using Catalog.ModelBuilder.Model;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.ProductTestCases;

public class DeleteTests : IntegrationTestBase
{
    private Guid _productId;
    private Guid _otherProductId;

    // Seeded through the DbContext rather than the repository: this suite tests Delete, so it
    // must not fail because Add is broken.
    [SetUp]
    public async Task SeedProductsAsync()
    {
        Product product = new ProductBuilder().WithName("To delete").Build();
        Product otherProduct = new ProductBuilder().WithName("To keep").Build();

        _productId = product.Id;
        _otherProductId = otherProduct.Id;

        await Context.Products.AddRangeAsync(product, otherProduct);
        await Context.SaveChangesAsync();
    }

    [Test]
    public async Task Should_Delete_Product()
    {
        // Arrange
        ProductRepository repository = new(Context);
        Product? productToDelete = await repository.ReadTracked(_productId);

        // Act
        repository.Delete(productToDelete!);
        await repository.SaveChangeAsync();

        // Assert - a fresh context, so the row is really read back from Postgres instead of
        // being answered from the change tracker of the context that performed the delete.
        await using CatalogDbContext verifyContext = DatabaseFixture.CreateContext();
        Product? deleted = await verifyContext.Products
            .FirstOrDefaultAsync(p => p.Id == _productId);

        Assert.That(deleted, Is.Null);
    }

    [Test]
    public async Task Should_Not_Delete_Other_Products()
    {
        // Arrange
        ProductRepository repository = new(Context);
        Product? productToDelete = await repository.ReadTracked(_productId);

        // Act
        repository.Delete(productToDelete!);
        await repository.SaveChangeAsync();

        // Assert - deleting one row must leave the rest of the table untouched.
        await using CatalogDbContext verifyContext = DatabaseFixture.CreateContext();
        List<Product> remaining = await verifyContext.Products.ToListAsync();

        Assert.That(remaining, Has.Count.EqualTo(1));
        Assert.That(remaining[0].Id, Is.EqualTo(_otherProductId));
    }

    [Test]
    public async Task Should_Not_Delete_Product_When_Changes_Are_Not_Saved()
    {
        // Arrange
        ProductRepository repository = new(Context);
        Product? productToDelete = await repository.ReadTracked(_productId);

        // Act - Delete only marks the entity as removed in the change tracker; nothing reaches
        // the database until SaveChangeAsync is called, which is deliberately skipped here.
        repository.Delete(productToDelete!);

        // Assert
        await using CatalogDbContext verifyContext = DatabaseFixture.CreateContext();
        Product? stillThere = await verifyContext.Products
            .FirstOrDefaultAsync(p => p.Id == _productId);

        Assert.That(stillThere, Is.Not.Null);
    }
}
