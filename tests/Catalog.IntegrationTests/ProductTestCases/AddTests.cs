using Catalog.Api.Data;
using Catalog.Api.Models;
using Catalog.IntegrationTests.Infrastructure;
using Catalog.ModelBuilder.Model;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.ProductTestCases;

public class AddTests : IntegrationTestBase
{

    [Test]
    public async Task Should_Add_Product()
    {
        // Arrange
        Product product = new ProductBuilder().Build();

        ProductRepository repository = new(Context);

        // Act
        await repository.Add(product);
        await repository.SaveChangeAsync();

        // Assert
        Product? saved = await Context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Name, Is.EqualTo(product.Name));
        Assert.That(saved.Price, Is.EqualTo(product.Price));
    }
}
