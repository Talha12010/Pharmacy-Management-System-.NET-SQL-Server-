using Microsoft.EntityFrameworkCore;

namespace PharmacyManagement.Application.Common;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        => new() { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };

    public static PagedResult<T> Empty(int page = 1, int pageSize = 20)
        => new() { Items = Array.Empty<T>(), Page = page, PageSize = pageSize, TotalCount = 0 };
}

public static class QueryablePagingExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return PagedResult<T>.Create(items, page, pageSize, total);
    }

    public static IQueryable<T> ApplyOrdering<T>(
        this IQueryable<T> query, string? sortBy, bool descending, string defaultSortBy)
    {
        var propertyName = string.IsNullOrWhiteSpace(sortBy) ? defaultSortBy : sortBy;
        var property = typeof(T).GetProperty(
            propertyName,
            System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (property is null)
        {
            property = typeof(T).GetProperty(defaultSortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        }

        if (property is null)
        {
            return query;
        }

        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
        var member = System.Linq.Expressions.Expression.Property(parameter, property);
        var lambda = System.Linq.Expressions.Expression.Lambda(member, parameter);
        var method = descending ? "OrderByDescending" : "OrderBy";
        var call = System.Linq.Expressions.Expression.Call(
            typeof(Queryable), method, new[] { typeof(T), property.PropertyType }, query.Expression, System.Linq.Expressions.Expression.Quote(lambda));
        return query.Provider.CreateQuery<T>(call);
    }
}
