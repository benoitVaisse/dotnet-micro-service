using Catalog.Api.Models.Filter;
using System.Linq.Expressions;

namespace Catalog.Api.Models.CRUD;


public static class Filter
{
    public static IQueryable<T> FilterBy<T>(this IQueryable<T> query, FilterRequest filterRequest) where T : IFilterable
    {
        if (!string.IsNullOrWhiteSpace(filterRequest.Name))
            query = query.Where(p => p.Name.Contains(filterRequest.Name));

        return query;
    }

    public static IQueryable<T> SortFilter<T>(this IQueryable<T> query, FilterRequest filterRequest) where T : ISortable
    {
        if (filterRequest.Sort == null)
            return query;
        IOrderedQueryable<T>? ordered = null;
        foreach ((SortBy sortBy, Sort sort) in filterRequest.Sort)
        {
            Expression<Func<T, object>> selector = sortBy switch
            {
                SortBy.Name => (p => p.Name),
                SortBy.Price => (p => p.Price),
                _ => throw new ArgumentOutOfRangeException(message: "sortBy not on range", null)
            };
            ordered = (ordered, sort) switch
            {
                (null, Sort.Asc) => query.OrderBy(selector),
                (null, Sort.Desc) => query.OrderByDescending(selector),
                (not null, Sort.Asc) => ordered.ThenBy(selector),
                (not null, Sort.Desc) => ordered.ThenByDescending(selector),
                _ => throw new ArgumentOutOfRangeException(message: "sort not on range", null)
            };
        }

        return ordered ?? query;
    }

    public static IQueryable<T> Pagination<T>(this IQueryable<T> query, FilterRequest filterRequest) where T : IEntity
    {

        IQueryable<T> newQuery = query.Skip((filterRequest.PageIndex - 1) * filterRequest.PageSize).Take(filterRequest.PageSize);

        return newQuery;
    }
}
