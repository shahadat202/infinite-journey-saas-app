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

public sealed class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message = "You do not have permission to perform this action.")
        : base(message, 403, "FORBIDDEN")
    {
    }
}
