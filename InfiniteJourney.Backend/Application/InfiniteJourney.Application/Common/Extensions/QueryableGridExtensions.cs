using System.Linq.Expressions;
using InfiniteJourney.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Common.Extensions;

public static class QueryableGridExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        GridQuery grid,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var total = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip(grid.PageIndex * grid.PageSize)
            .Take(grid.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Data = data,
            PageIndex = grid.PageIndex,
            PageSize = grid.PageSize,
            Total = total
        };
    }

    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? search,
        params Expression<Func<T, string>>[] searchableFields)
    {
        if (string.IsNullOrWhiteSpace(search) || searchableFields.Length == 0)
            return query;

        var term = search.Trim().ToLower();
        var parameter = Expression.Parameter(typeof(T), "e");
        Expression? combined = null;

        foreach (var field in searchableFields)
        {
            var body = new ReplaceParameterVisitor(field.Parameters[0], parameter).Visit(field.Body)!;
            var toLower = Expression.Call(body, nameof(string.ToLower), Type.EmptyTypes);
            var contains = Expression.Call(
                toLower,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(term));

            combined = combined is null ? contains : Expression.OrElse(combined, contains);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combined!, parameter);
        return query.Where(lambda);
    }

    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        GridQuery grid,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>> defaultSort)
    {
        var key = grid.SortBy?.Trim().ToLowerInvariant();
        var sortExpr = key is not null && sortMap.TryGetValue(key, out var mapped)
            ? mapped
            : defaultSort;

        return grid.IsDescending
            ? query.OrderByDescending(sortExpr)
            : query.OrderBy(sortExpr);
    }

    private sealed class ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node)!;
    }
}
