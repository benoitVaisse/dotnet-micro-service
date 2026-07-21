using Catalog.Api.Data;
using Catalog.Api.Models;
using Catalog.Api.Models.Filter;
using Catalog.IntegrationTests.Infrastructure;
using Catalog.ModelBuilder.Model;

namespace Catalog.IntegrationTests.ProductTestCases;

public class GetFilteredTests : IntegrationTestBase
{
    private ProductRepository _repository = null!;

    // Assigned in [SetUp], never in a field initializer or a constructor: NUnit reuses the
    // same test class instance for every [Test], so anything built once would leak state
    // between tests.
    [SetUp]
    public async Task SeedProductsAsync()
    {
        _repository = new ProductRepository(Context);

        // Names deliberately start with distinct letters of the same case, and prices are far
        // apart, so the expected order holds whatever collation the Postgres image uses.
        // "keyboard" appears twice so the name filter has something to exclude.
        Product alpha = new ProductBuilder().WithName("Alpha keyboard").WithPrice(10.00m).Build();
        Product beta = new ProductBuilder().WithName("Beta keyboard").WithPrice(30.00m).Build();
        Product gamma = new ProductBuilder().WithName("Gamma mouse").WithPrice(20.00m).Build();

        await Context.Products.AddRangeAsync(alpha, beta, gamma);
        await Context.SaveChangesAsync();
    }

    [Test]
    public async Task Should_Return_All_Products_When_No_Filter_Is_Applied()
    {
        // Arrange - the record defaults (PageSize 10, PageIndex 1) still paginate, but the
        // page is large enough to hold everything.
        FilterRequest request = new();

        // Act
        IEnumerable<Product> products = await _repository.GetFiltered(request);

        // Assert
        Assert.That(products.ToList(), Has.Count.EqualTo(3));
    }

    [Test]
    public async Task Should_Filter_By_Name()
    {
        // Arrange - FilterBy translates to a SQL LIKE '%keyboard%', so this matches on a
        // fragment rather than the whole name.
        FilterRequest request = new(Name: "keyboard");

        // Act
        List<Product> products = (await _repository.GetFiltered(request)).ToList();

        // Assert
        Assert.That(products, Has.Count.EqualTo(2));
        Assert.That(products.Select(p => p.Name),
            Is.EquivalentTo(["Alpha keyboard", "Beta keyboard"]));
    }

    [Test]
    public async Task Should_Not_Match_Name_With_A_Different_Case()
    {
        // Arrange - the seeded products all spell it "keyboard" in lower case.
        FilterRequest request = new(Name: "Keyboard");

        // Act
        IEnumerable<Product> products = await _repository.GetFiltered(request);

        // Assert - documents current behaviour rather than desired behaviour: string.Contains
        // is translated to a SQL LIKE '%Keyboard%', and LIKE is case-sensitive on Postgres
        // (unlike SQL Server, whose default collation is case-insensitive). For a product
        // search this is very likely a bug - EF.Functions.ILike is the Npgsql answer - and
        // this test is what will turn red the day the behaviour is changed on purpose.
        Assert.That(products, Is.Empty);
    }

    [Test]
    public async Task Should_Return_Empty_When_Name_Matches_Nothing()
    {
        // Arrange
        FilterRequest request = new(Name: "does-not-exist");

        // Act
        IEnumerable<Product> products = await _repository.GetFiltered(request);

        // Assert - an empty result, not null: the caller should never have to null-check.
        Assert.That(products, Is.Empty);
    }

    [Test]
    public async Task Should_Sort_By_Price_Ascending()
    {
        // Arrange
        FilterRequest request = new(Sort: new() { [SortBy.Price] = Sort.Asc });

        // Act
        List<Product> products = [.. (await _repository.GetFiltered(request))];

        // Assert - Is.EqualTo on a list compares element by element in order, which is exactly
        // what a sort test must check. Is.EquivalentTo would ignore the order entirely.
        Assert.That(products.Select(p => p.Price).ToList(),
            Is.EqualTo([10.00m, 20.00m, 30.00m]));
    }

    [Test]
    public async Task Should_Sort_By_Price_Descending()
    {
        // Arrange
        FilterRequest request = new(Sort: new() { [SortBy.Price] = Sort.Desc });

        // Act
        List<Product> products = (await _repository.GetFiltered(request)).ToList();

        // Assert
        Assert.That(products.Select(p => p.Price).ToList(),
            Is.EqualTo([30.00m, 20.00m, 10.00m]));
    }

    [Test]
    public async Task Should_Sort_By_Name_Ascending()
    {
        // Arrange
        FilterRequest request = new(Sort: new() { [SortBy.Name] = Sort.Asc });

        // Act
        List<Product> products = (await _repository.GetFiltered(request)).ToList();

        // Assert
        Assert.That(products.Select(p => p.Name).ToList(),
            Is.EqualTo(["Alpha keyboard", "Beta keyboard", "Gamma mouse"]));
    }

    [Test]
    public async Task Should_Return_First_Page()
    {
        // Arrange - always paired with a sort: without ORDER BY, Postgres makes no promise
        // about row order, so paging over an unsorted query is not reproducible.
        FilterRequest request = new(
            PageSize: 2,
            PageIndex: 1,
            Sort: new() { [SortBy.Name] = Sort.Asc });

        // Act
        List<Product> products = (await _repository.GetFiltered(request)).ToList();

        // Assert
        Assert.That(products.Select(p => p.Name).ToList(),
            Is.EqualTo(["Alpha keyboard", "Beta keyboard"]));
    }

    [Test]
    public async Task Should_Return_Second_Page()
    {
        // Arrange - PageIndex is 1-based: page 2 skips (2 - 1) * 2 = 2 rows.
        FilterRequest request = new(
            PageSize: 2,
            PageIndex: 2,
            Sort: new() { [SortBy.Name] = Sort.Asc });

        // Act
        List<Product> products = (await _repository.GetFiltered(request)).ToList();

        // Assert - the last page holds the remainder only, not a full page.
        Assert.That(products.Select(p => p.Name).ToList(),
            Is.EqualTo(["Gamma mouse"]));
    }

    [Test]
    public async Task Should_Return_Empty_When_Page_Is_Out_Of_Range()
    {
        // Arrange
        FilterRequest request = new(
            PageSize: 2,
            PageIndex: 99,
            Sort: new() { [SortBy.Name] = Sort.Asc });

        // Act
        IEnumerable<Product> products = await _repository.GetFiltered(request);

        // Assert - skipping past the end returns nothing rather than failing.
        Assert.That(products, Is.Empty);
    }

    [Test]
    public async Task Should_Combine_Name_Filter_And_Sort()
    {
        // Arrange - filter then sort: the ORDER BY must apply to the filtered set, not to the
        // whole table.
        FilterRequest request = new(
            Name: "keyboard",
            Sort: new() { [SortBy.Price] = Sort.Desc });

        // Act
        List<Product> products = (await _repository.GetFiltered(request)).ToList();

        // Assert
        Assert.That(products.Select(p => p.Name).ToList(),
            Is.EqualTo(["Beta keyboard", "Alpha keyboard"]));
    }
}
