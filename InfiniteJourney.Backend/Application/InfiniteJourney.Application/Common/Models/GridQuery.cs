namespace InfiniteJourney.Application.Common.Models;

/// <summary>
/// Standard grid/query parameters for paginated list endpoints.
/// Bind from query string: ?pageIndex=0&amp;pageSize=10&amp;search=water&amp;sortBy=title&amp;sortDirection=asc
/// </summary>
public class GridQuery
{
    private int _pageIndex;
    private int _pageSize = 10;

    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 0 ? 0 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            <= 0 => 10,
            > 100 => 100,
            _ => value
        };
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "desc";

    public bool IsDescending =>
        string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
}
