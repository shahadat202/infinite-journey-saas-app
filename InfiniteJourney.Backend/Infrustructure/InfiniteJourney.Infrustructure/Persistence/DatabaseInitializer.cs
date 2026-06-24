using InfiniteJourney.Domain.Aggregates.Campaign;
using InfiniteJourney.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InfiniteJourney.Infrustructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, bool seedDevelopmentData = false)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await context.Database.MigrateAsync();

        if (!seedDevelopmentData)
            return;

        if (await context.Tenants.AnyAsync())
            return;

        logger.LogInformation("Seeding development tenants...");

        var tenantOne = Tenant.Create("Hope Foundation", "hope");
        tenantOne.Activate();

        var tenantTwo = Tenant.Create("Community Relief", "relief");
        tenantTwo.Activate();

        context.Tenants.AddRange(tenantOne, tenantTwo);
        context.Themes.AddRange(
            Theme.CreateDefault(tenantOne.Id),
            Theme.CreateDefault(tenantTwo.Id));
        context.ModuleActivations.AddRange(
            ModuleActivation.Create(tenantOne.Id, "Campaigns"),
            ModuleActivation.Create(tenantOne.Id, "Donations"),
            ModuleActivation.Create(tenantTwo.Id, "Campaigns"),
            ModuleActivation.Create(tenantTwo.Id, "Donations"));

        var hopeCampaign = Campaign.Create(
            tenantOne.Id,
            "Clean Water for Rural Communities",
            "Providing sustainable clean water access to underserved villages.",
            50000m);
        hopeCampaign.Activate();

        var reliefCampaign = Campaign.Create(
            tenantTwo.Id,
            "Emergency Food Relief",
            "Delivering essential food supplies to families in crisis.",
            25000m);
        reliefCampaign.Activate();

        context.Campaigns.AddRange(hopeCampaign, reliefCampaign);

        await context.SaveChangesAsync();
        logger.LogInformation("Development data seeded. Tenants: hope.localhost, relief.localhost");
    }
}
