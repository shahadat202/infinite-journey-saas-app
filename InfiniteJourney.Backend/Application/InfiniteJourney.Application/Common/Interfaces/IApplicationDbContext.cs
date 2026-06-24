using InfiniteJourney.Domain.Aggregates.Campaign;
using InfiniteJourney.Domain.Aggregates.Tenant;
using InfiniteJourney.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Theme> Themes { get; }
    DbSet<ModuleActivation> ModuleActivations { get; }
    DbSet<User> Users { get; }
    DbSet<Membership> Memberships { get; }
    DbSet<Campaign> Campaigns { get; }
    DbSet<Donation> Donations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
