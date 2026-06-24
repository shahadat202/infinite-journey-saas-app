using System;
using System.Collections.Generic;
using InfiniteJourney.Domain.Common;
using InfiniteJourney.Global.Shared.Enums;

namespace InfiniteJourney.Domain.Aggregates.Tenant;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Subdomain { get; private set; } = string.Empty;
    public string? CustomDomain { get; private set; }
    public TenantStatus Status { get; private set; } = TenantStatus.Provisioning;
    public string? ConnectionString { get; private set; } // For Option D - Dedicated database connection

    // Navigation properties can be added here or kept clean of deep EF references
    
    private Tenant() { } // Required for EF Core

    public static Tenant Create(string name, string subdomain, string? customDomain = null, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(subdomain))
            throw new ArgumentException("Subdomain cannot be empty.", nameof(subdomain));

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Subdomain = subdomain.ToLowerInvariant().Trim(),
            CustomDomain = customDomain?.ToLowerInvariant().Trim(),
            Status = TenantStatus.Provisioning,
            ConnectionString = connectionString,
            CreatedAt = DateTimeOffset.UtcNow
        };

        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, tenant.Name, tenant.Subdomain));
        return tenant;
    }

    public void UpdateDetails(string name, string? customDomain)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name cannot be empty.", nameof(name));

        Name = name;
        CustomDomain = customDomain?.ToLowerInvariant().Trim();
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (Status == TenantStatus.Active) return;
        Status = TenantStatus.Active;
        LastModifiedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TenantActivatedEvent(Id));
    }

    public void Suspend()
    {
        if (Status == TenantStatus.Suspended) return;
        Status = TenantStatus.Suspended;
        LastModifiedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TenantSuspendedEvent(Id));
    }
}

public record TenantCreatedEvent(Guid TenantId, string Name, string Subdomain) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public record TenantActivatedEvent(Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public record TenantSuspendedEvent(Guid TenantId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
