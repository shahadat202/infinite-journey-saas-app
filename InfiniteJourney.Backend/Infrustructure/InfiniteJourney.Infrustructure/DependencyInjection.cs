using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Infrustructure.Identity;
using InfiniteJourney.Infrustructure.MultiTenancy;
using InfiniteJourney.Infrustructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteJourney.Infrustructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddPersistence(configuration);
        services.AddKeycloakAuthentication(configuration);

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<ITenantResolver, TenantResolver>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
