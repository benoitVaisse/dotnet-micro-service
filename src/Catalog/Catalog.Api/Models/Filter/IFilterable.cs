namespace Catalog.Api.Models.Filter;

public interface IFilterable
{
    public string Name { get; set; }
}

public interface ISortable
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
