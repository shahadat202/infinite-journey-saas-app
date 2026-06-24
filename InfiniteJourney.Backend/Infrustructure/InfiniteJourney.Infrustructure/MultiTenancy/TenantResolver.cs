using InfiniteJourney.Infrustructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InfiniteJourney.Infrustructure.MultiTenancy;

public sealed class TenantResolver : ITenantResolver
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public TenantResolver(ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<TenantResolutionResult?> ResolveAsync(string host, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
            return null;

        var normalizedHost = host.Split(':')[0].ToLowerInvariant();
        var platformDomain = _configuration["MultiTenancy:PlatformDomain"]?.ToLowerInvariant() ?? "infinitejourney.com";

        Domain.Aggregates.Tenant.Tenant? tenant;

        if (normalizedHost.EndsWith($".{platformDomain}", StringComparison.Ordinal))
        {
            var subdomain = normalizedHost[..^(platformDomain.Length + 1)];
            if (string.IsNullOrWhiteSpace(subdomain) || subdomain.Contains('.'))
                return null;

            tenant = await _dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);
        }
        else if (normalizedHost.EndsWith(".localhost", StringComparison.Ordinal))
        {
            var subdomain = normalizedHost[..^".localhost".Length];
            if (string.IsNullOrWhiteSpace(subdomain) || subdomain.Contains('.'))
                return null;

            tenant = await _dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);
        }
        else
        {
            tenant = await _dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.CustomDomain == normalizedHost, cancellationToken);
        }

        if (tenant is null || tenant.Status != Domain.Aggregates.Tenant.TenantStatus.Active)
            return null;

        var enabledModules = await _dbContext.ModuleActivations
            .AsNoTracking()
            .Where(m => m.TenantId == tenant.Id && m.IsEnabled)
            .Select(m => m.ModuleKey)
            .ToListAsync(cancellationToken);

        var connectionString = tenant.ConnectionString
            ?? _configuration.GetConnectionString("DefaultConnection");

        return new TenantResolutionResult(
            tenant.Id,
            tenant.Name,
            tenant.Subdomain,
            connectionString,
            enabledModules);
    }
}
