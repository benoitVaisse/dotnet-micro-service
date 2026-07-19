using Catalog.Api.Models.Filter;
using System.ComponentModel.DataAnnotations;

namespace Catalog.Api.Models;

public partial class Product : IEntity, IFilterable, ISortable
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int AvailableStock { get; set; }
}
