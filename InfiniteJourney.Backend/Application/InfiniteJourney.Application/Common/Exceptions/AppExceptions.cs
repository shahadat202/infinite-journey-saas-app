namespace InfiniteJourney.Application.Common.Exceptions;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    protected AppException(string message, int statusCode, string errorCode)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(message, 404, "NOT_FOUND")
    {
    }
}

public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message)
        : base(message, 409, "BUSINESS_RULE_VIOLATION")
    {
    }
}

/// <summary>
/// Thrown when an action cannot be performed because of a state conflict
/// on an existing resource — e.g. deleting a campaign that has donations,
/// or activating one that is already active.
/// Maps to HTTP 409 Conflict with a distinct error code from general business
/// rule violations, making it easier to handle on the client.
/// </summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, 409, "CONFLICT")
    {
    }
}

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message = "You do not have permission to perform this action.")
        : base(message, 403, "FORBIDDEN")
    {
    }
}
