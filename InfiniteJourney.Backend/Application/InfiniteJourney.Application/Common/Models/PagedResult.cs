namespace InfiniteJourney.Application.Common.Models;

/// <summary>
/// Standard paginated API response — matches global grid contract.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
}
