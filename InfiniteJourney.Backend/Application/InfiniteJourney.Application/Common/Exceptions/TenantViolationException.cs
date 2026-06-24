namespace InfiniteJourney.Application.Common.Exceptions;

public class TenantViolationException : Exception
{
    public TenantViolationException(string message) : base(message)
    {
    }
}
