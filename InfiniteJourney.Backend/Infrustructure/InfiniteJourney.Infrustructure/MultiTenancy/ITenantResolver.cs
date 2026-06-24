using InfiniteJourney.Domain.Aggregates.Tenant;

namespace InfiniteJourney.Infrustructure.MultiTenancy;

public interface ITenantResolver
{
    Task<TenantResolutionResult?> ResolveAsync(string host, CancellationToken cancellationToken = default);
}

public sealed record TenantResolutionResult(
    Guid TenantId,
    string TenantName,
    string Subdomain,
    string? ConnectionString,
    IReadOnlyList<string> EnabledModules);
