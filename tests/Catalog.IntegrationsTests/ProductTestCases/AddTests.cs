using Catalog.Api.Data;
using Catalog.Api.Models;
using Catalog.IntegrationsTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationsTests.ProductTestCases;

public class AddTests : IntegrationTestBase
{
    public AddTests()
    {

    }

    [Test]
    public async Task Should_Add_Product()
    {
        // Arrange
        Product product = new()
        {
            Id = Guid.NewGuid(),
            Name = "Clavier mécanique",
            Price = 49.99m,
            AvailableStock = 10
        };
        ProductRepository repository = new(Context);

        // Act
        await repository.Add(product);
        await repository.SaveChangeAsync();

        // Assert
        Product? saved = await Context.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        Assert.That(saved, Is.Not.Null);
        Assert.That(saved!.Name, Is.EqualTo("Clavier mécanique"));
        Assert.That(saved.Price, Is.EqualTo(49.99m));
    }
}
