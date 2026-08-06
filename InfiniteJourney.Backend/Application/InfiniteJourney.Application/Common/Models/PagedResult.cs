namespace InfiniteJourney.Application.Common.Models;

/// <summary>
/// Standard paginated API response. All list endpoints return this shape.
/// </summary>
public sealed class PagedResult<T>
{
    /// <summary>Current zero-based page index.</summary>
    public int PageIndex { get; init; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; init; }

    /// <summary>Total number of matching records across all pages.</summary>
    public int Total { get; init; }

    /// <summary>Total number of pages. Computed from Total / PageSize.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

    /// <summary>Items in the current page.</summary>
    public IReadOnlyList<T> Data { get; init; } = [];

    // -------------------------------------------------------------------------
    // Factory helpers — use these instead of constructing inline every time
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a paged result directly from already-fetched data.
    /// </summary>
    public static PagedResult<T> Create(IReadOnlyList<T> data, int total, GridQuery grid) =>
        new()
        {
            Data = data,
            Total = total,
            PageIndex = grid.PageIndex,
            PageSize = grid.PageSize
        };

    /// <summary>
    /// Projects each item in an existing paged result to a different DTO type.
    /// Use when the DB projection and the API DTO type differ.
    /// </summary>
    public PagedResult<TDto> Map<TDto>(Func<T, TDto> selector) =>
        new()
        {
            Data = Data.Select(selector).ToList(),
            Total = Total,
            PageIndex = PageIndex,
            PageSize = PageSize
        };
}
