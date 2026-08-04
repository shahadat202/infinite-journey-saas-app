namespace InfiniteJourney.Application.Common.Models;

public sealed class ApiErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
    public string? TraceId { get; init; }
    public IReadOnlyList<ApiFieldError>? Errors { get; init; }
}

public sealed record ApiFieldError(string Field, string Message);
