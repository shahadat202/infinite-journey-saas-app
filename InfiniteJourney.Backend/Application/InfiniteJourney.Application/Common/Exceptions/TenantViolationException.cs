namespace InfiniteJourney.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request attempts to access or mutate data belonging to a
/// different tenant than the one resolved for the current request.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class TenantViolationException : AppException
{
    public TenantViolationException(string message)
        : base(message, statusCode: 403, errorCode: "TENANT_VIOLATION")
    {
    }

    public TenantViolationException(Guid requestedTenantId, Guid currentTenantId)
        : this($"Access denied: resource belongs to tenant '{requestedTenantId}', " +
               $"but the current tenant is '{currentTenantId}'.")
    {
    }
}
