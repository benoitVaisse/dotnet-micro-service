using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Catalog.Api.Models.Filter;

public record FilterRequest(
    [Description("Number of items to return in a single page of results")]
    [DefaultValue(10)]
    int PageSize = 10,

    [Description("The index of the page of results to return")]
    [DefaultValue(1)]
    int PageIndex = 1,

    [Description("Sort filter of result")]
    [DefaultValue(null)]
    Dictionary<SortBy, Sort>? Sort = null,

    [Description("Name for filter result")]
    [DefaultValue(null)]
    string? Name = null
);

[JsonConverter(typeof(JsonStringEnumConverter<Sort>))]
public enum Sort
{
    Asc,
    Desc
}

[JsonConverter(typeof(JsonStringEnumConverter<SortBy>))]
public enum SortBy
{
    Name,
    Price
}