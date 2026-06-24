using InfiniteJourney.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TenantEntity = InfiniteJourney.Domain.Aggregates.Tenant.Tenant;

namespace InfiniteJourney.Infrustructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<TenantEntity>
{
    public void Configure(EntityTypeBuilder<TenantEntity> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Subdomain)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();

        builder.Property(t => t.CustomDomain)
            .HasMaxLength(255);

        builder.HasIndex(t => t.CustomDomain)
            .IsUnique()
            .HasFilter("\"CustomDomain\" IS NOT NULL");

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(t => t.ConnectionString)
            .HasMaxLength(500);
    }
}
