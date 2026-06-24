using System;
using System.Collections.Generic;
using System.Linq;
using InfiniteJourney.Domain.Common;

namespace InfiniteJourney.Domain.Aggregates.User;

public class User : BaseEntity
{
    public string KeycloakUserId { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    private readonly List<Membership> _memberships = new();
    public virtual IReadOnlyCollection<Membership> Memberships => _memberships.AsReadOnly();

    private User() { } // Required for EF Core

    public static User Create(string keycloakUserId, string email, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(keycloakUserId))
            throw new ArgumentException("Keycloak user ID cannot be empty.", nameof(keycloakUserId));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        var user = new User
        {
            Id = Guid.NewGuid(),
            KeycloakUserId = keycloakUserId,
            Email = email.ToLowerInvariant().Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email));
        return user;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void AddMembership(Guid tenantId, string roleName)
    {
        if (_memberships.Any(m => m.TenantId == tenantId))
            throw new InvalidOperationException($"User is already a member of tenant {tenantId}");

        var membership = Membership.Create(tenantId, Id, roleName);
        _memberships.Add(membership);

        AddDomainEvent(new UserMembershipAddedEvent(Id, tenantId, roleName));
    }
}

public record UserCreatedEvent(Guid UserId, string Email) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public record UserMembershipAddedEvent(Guid UserId, Guid TenantId, string RoleName) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
