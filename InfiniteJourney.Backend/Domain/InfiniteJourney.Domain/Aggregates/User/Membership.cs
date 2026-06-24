using System;
using InfiniteJourney.Domain.Common;
using InfiniteJourney.Global.Shared.Enums;

namespace InfiniteJourney.Domain.Aggregates.User;

public class Membership : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Guid UserId { get; private set; }
    public string RoleName { get; private set; } = string.Empty;
    public MembershipStatus Status { get; private set; } = MembershipStatus.Pending;
    public DateTimeOffset JoinedAt { get; private set; } = DateTimeOffset.UtcNow;

    private Membership() { } // Required for EF Core

    public static Membership Create(Guid tenantId, Guid userId, string roleName)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        return new Membership
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UserId = userId,
            RoleName = roleName,
            Status = MembershipStatus.Pending,
            JoinedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        RoleName = roleName;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        Status = MembershipStatus.Active;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend()
    {
        Status = MembershipStatus.Suspended;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
}
