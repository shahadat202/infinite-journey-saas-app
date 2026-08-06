using System.Linq.Expressions;
using InfiniteJourney.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Common.Extensions;

public static class QueryableGridExtensions
{
    // -------------------------------------------------------------------------
    // Pagination
    // -------------------------------------------------------------------------

    /// <summary>
    /// Executes a count + paged fetch and returns <see cref="PagedResult{T}"/>.
    /// Use when the EF projection type and the DTO type are the same.
    /// </summary>
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

        return PagedResult<T>.Create(data, total, grid);
    }

    /// <summary>
    /// Executes a count + paged fetch, then projects each entity to a DTO in a
    /// single database round-trip. Eliminates the double-pass pattern where callers
    /// would call <see cref="ToPagedResultAsync{T}"/> and immediately call
    /// <c>paged.Map(...)</c> afterwards.
    /// </summary>
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TSource, TDto>(
        this IQueryable<TSource> query,
        GridQuery grid,
        Func<TSource, TDto> selector,
        CancellationToken cancellationToken = default)
        where TSource : class
    {
        var total = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip(grid.PageIndex * grid.PageSize)
            .Take(grid.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<TDto>.Create(
            data.Select(selector).ToList(),
            total,
            grid);
    }

    // -------------------------------------------------------------------------
    // Search
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies a case-insensitive OR search across all provided string fields.
    /// No-ops when <paramref name="search"/> is null or whitespace.
    /// </summary>
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
            var body = new ReplaceParameterVisitor(field.Parameters[0], parameter)
                .Visit(field.Body)!;

            var toLower = Expression.Call(body, nameof(string.ToLower), Type.EmptyTypes);
            var contains = Expression.Call(
                toLower,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(term));

            combined = combined is null
                ? contains
                : Expression.OrElse(combined, contains);
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(combined!, parameter));
    }

    // -------------------------------------------------------------------------
    // Sorting
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies a sort from the <see cref="GridQuery.SortBy"/> field against a
    /// caller-supplied map of column-name → expression. Falls back to
    /// <paramref name="defaultSort"/> when the field is absent or unrecognised.
    /// </summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        GridQuery grid,
        IReadOnlyDictionary<string, Expression<Func<T, object>>> sortMap,
        Expression<Func<T, object>> defaultSort)
    {
        var key = grid.SortBy?.Trim().ToLowerInvariant();
        var expr = key is not null && sortMap.TryGetValue(key, out var mapped)
            ? mapped
            : defaultSort;

        return grid.IsDescending
            ? query.OrderByDescending(expr)
            : query.OrderBy(expr);
    }

    // -------------------------------------------------------------------------
    // Internal helpers
    // -------------------------------------------------------------------------

    private sealed class ReplaceParameterVisitor(
        ParameterExpression oldParam,
        ParameterExpression newParam) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == oldParam ? newParam : base.VisitParameter(node)!;
    }
}
