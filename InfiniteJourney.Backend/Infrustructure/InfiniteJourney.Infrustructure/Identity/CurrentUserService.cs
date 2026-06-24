using System.Security.Claims;
using InfiniteJourney.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InfiniteJourney.Infrustructure.Identity;

public sealed class CurrentUserService : ICurrentUserService
{
    private static readonly string[] StaffRoles =
    [
        "OrganizationAdmin",
        "OrgAdmin",
        "Organization Owner",
        "Staff",
        "Volunteer Coordinator",
        "Content Manager",
        "Finance Manager"
    ];

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Email =>
        User?.FindFirstValue("email") ?? User?.FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInTenantRole(Guid tenantId, params string[] roles)
    {
        if (!IsAuthenticated)
            return false;

        var effectiveRoles = roles.Length > 0 ? roles : StaffRoles;

        if (User!.Claims.Any(c =>
                (c.Type == ClaimTypes.Role || c.Type == "roles") &&
                effectiveRoles.Contains(c.Value, StringComparer.OrdinalIgnoreCase)))
        {
            return true;
        }

        var tenantRolesClaim = User.FindFirst("tenant_roles")?.Value;
        if (string.IsNullOrWhiteSpace(tenantRolesClaim))
            return User.IsInRole("OrganizationAdmin") || User.IsInRole("OrgAdmin");

        return tenantRolesClaim.Contains(tenantId.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
