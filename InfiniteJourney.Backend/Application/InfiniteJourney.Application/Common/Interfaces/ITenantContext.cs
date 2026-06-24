namespace InfiniteJourney.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    string? TenantName { get; }
    string? Subdomain { get; }
    string? ConnectionString { get; }
    bool IsResolved { get; }
    bool IsFeatureEnabled(string featureName);
}
