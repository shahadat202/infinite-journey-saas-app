using InfiniteJourney.Application.Common.Exceptions;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Domain.Aggregates.Campaign;
using InfiniteJourney.Domain.Aggregates.Tenant;
using InfiniteJourney.Infrustructure.MultiTenancy;
using InfiniteJourney.Infrustructure.Persistence;
using InfiniteJourney.Infrustructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteJourney.Tests.MultiTenancy;

public class TenantIsolationTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid TenantB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public async Task QueryFilter_ReturnsOnlyCurrentTenantCampaigns()
    {
        await using var contextA = CreateContext(TenantA);
        await using var contextB = CreateContext(TenantB);

        contextA.Campaigns.Add(Campaign.Create(TenantA, "Tenant A Campaign", "Description", 1000m));
        contextB.Campaigns.Add(Campaign.Create(TenantB, "Tenant B Campaign", "Description", 2000m));

        await contextA.SaveChangesAsync();
        await contextB.SaveChangesAsync();

        var tenantACampaigns = await contextA.Campaigns.ToListAsync();
        var tenantBCampaigns = await contextB.Campaigns.ToListAsync();

        Assert.Single(tenantACampaigns);
        Assert.Equal("Tenant A Campaign", tenantACampaigns[0].Title);

        Assert.Single(tenantBCampaigns);
        Assert.Equal("Tenant B Campaign", tenantBCampaigns[0].Title);
    }

    [Fact]
    public async Task SaveChangesInterceptor_ThrowsOnCrossTenantModification()
    {
        await using var context = CreateContext(TenantA);

        var campaign = Campaign.Create(TenantA, "Campaign", "Description", 500m);
        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        var tracked = await context.Campaigns.FirstAsync();
        tracked.TenantId = TenantB;

        await Assert.ThrowsAsync<TenantViolationException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChangesInterceptor_AssignsTenantIdOnInsert()
    {
        await using var context = CreateContext(TenantA);

        var campaign = Campaign.Create(TenantA, "Auto Tenant Campaign", "Description", 750m);
        campaign.TenantId = Guid.Empty;

        context.Campaigns.Add(campaign);
        await context.SaveChangesAsync();

        Assert.Equal(TenantA, campaign.TenantId);
    }

    private static ApplicationDbContext CreateContext(Guid tenantId)
    {
        var tenantContext = new TenantContext();
        tenantContext.SetTenant(tenantId, "Test Tenant", "test", null, ["Campaigns"]);

        var services = new ServiceCollection();
        services.AddSingleton<ITenantContext>(tenantContext);
        services.AddScoped<TenantSaveChangesInterceptor>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"InfiniteJourneyTests_{tenantId}_{Guid.NewGuid()}")
            .AddInterceptors(new TenantSaveChangesInterceptor(tenantContext))
            .Options;

        return new ApplicationDbContext(options, tenantContext);
    }
}
