using Catalog.Api.Data;
using Catalog.Api.Models;
using Catalog.IntegrationTests.Infrastructure;
using Catalog.ModelBuilder.Model;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.ProductTestCases;

public class UpdateTests : IntegrationTestBase
{
    private Guid _productId;

    [SetUp]
    public async Task SetUp()
    {
        await AddProduct();
    }

    private async Task AddProduct()
    {
        Product product = new ProductBuilder().Build();
        _productId = product.Id;
        await Context.Products.AddAsync(product);
        await Context.SaveChangesAsync();
    }

    [Test]
    public async Task Should_Update_Product()
    {
        // Arrange
        ProductRepository repository = new(Context);
        Product? producToUpdate = await repository.ReadTracked(_productId);


        producToUpdate!.Name = "New name";
        // Act
        await repository.SaveChangeAsync();

        // Assert
        Product? productUpdated = await Context.Products
             .FirstOrDefaultAsync(p => p.Id == _productId);

        Assert.That(productUpdated!.Name, Is.EqualTo(producToUpdate.Name));
    }
}
