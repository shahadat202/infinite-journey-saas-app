using InfiniteJourney.Application.Common.Interfaces;

namespace InfiniteJourney.Infrustructure.MultiTenancy;

public sealed class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string? TenantName { get; private set; }
    public string? Subdomain { get; private set; }
    public string? ConnectionString { get; private set; }
    public bool IsResolved { get; private set; }

    private HashSet<string> _enabledModules = new(StringComparer.OrdinalIgnoreCase);

    public void SetTenant(
        Guid tenantId,
        string tenantName,
        string subdomain,
        string? connectionString,
        IEnumerable<string>? enabledModules = null)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        Subdomain = subdomain;
        ConnectionString = connectionString;
        _enabledModules = enabledModules is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(enabledModules, StringComparer.OrdinalIgnoreCase);
        IsResolved = true;
    }

    public bool IsFeatureEnabled(string featureName)
    {
        if (string.IsNullOrWhiteSpace(featureName))
            return false;

        return _enabledModules.Contains(featureName);
    }
}
