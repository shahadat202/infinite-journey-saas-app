using System;
using InfiniteJourney.Domain.Common;

namespace InfiniteJourney.Domain.Aggregates.Tenant;

public class ModuleActivation : BaseTenantEntity
{
    public string ModuleKey { get; private set; } = string.Empty;
    public bool IsEnabled { get; private set; }
    public string? ConfigurationJson { get; private set; }

    private ModuleActivation() { } // Required for EF Core

    public static ModuleActivation Create(Guid tenantId, string moduleKey, bool isEnabled = true, string? configJson = null)
    {
        if (string.IsNullOrWhiteSpace(moduleKey))
            throw new ArgumentException("Module key cannot be empty.", nameof(moduleKey));

        return new ModuleActivation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ModuleKey = moduleKey.Trim(),
            IsEnabled = isEnabled,
            ConfigurationJson = configJson,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Enable()
    {
        IsEnabled = true;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void Disable()
    {
        IsEnabled = false;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateConfiguration(string? configJson)
    {
        ConfigurationJson = configJson;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
}
