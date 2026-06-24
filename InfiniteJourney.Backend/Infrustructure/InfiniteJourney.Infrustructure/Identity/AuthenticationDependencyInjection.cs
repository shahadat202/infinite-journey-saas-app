using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace InfiniteJourney.Infrustructure.Identity;

public static class AuthenticationDependencyInjection
{
    public static IServiceCollection AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authority = configuration["Keycloak:Authority"]
            ?? throw new InvalidOperationException("Keycloak:Authority is not configured.");
        var audience = configuration["Keycloak:Audience"];

        var validateAudience = configuration.GetValue("Keycloak:ValidateAudience", false);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = configuration.GetValue("Keycloak:RequireHttpsMetadata", false);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = validateAudience,
                    ValidAudience = validateAudience ? audience : null,
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal is not null)
                            MapKeycloakRoles(context.Principal);
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/api"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("TenantStaff", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireRole(
                        "OrganizationAdmin",
                        "OrgAdmin",
                        "Organization Owner",
                        "Staff",
                        "Volunteer Coordinator",
                        "Content Manager",
                        "Finance Manager"));

        return services;
    }

    private static void MapKeycloakRoles(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
            return;

        AddRolesFromJson(identity, principal.FindFirst("realm_access")?.Value, "roles");

        var resourceAccess = principal.FindFirst("resource_access")?.Value;
        if (string.IsNullOrWhiteSpace(resourceAccess))
            return;

        using var resourceDoc = JsonDocument.Parse(resourceAccess);
        foreach (var client in resourceDoc.RootElement.EnumerateObject())
        {
            if (client.Value.TryGetProperty("roles", out var clientRoles))
            {
                foreach (var role in clientRoles.EnumerateArray())
                {
                    var roleName = role.GetString();
                    if (!string.IsNullOrWhiteSpace(roleName))
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
    }

    private static void AddRolesFromJson(ClaimsIdentity identity, string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty(propertyName, out var roles))
            return;

        foreach (var role in roles.EnumerateArray())
        {
            var roleName = role.GetString();
            if (!string.IsNullOrWhiteSpace(roleName))
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
        }
    }
}
