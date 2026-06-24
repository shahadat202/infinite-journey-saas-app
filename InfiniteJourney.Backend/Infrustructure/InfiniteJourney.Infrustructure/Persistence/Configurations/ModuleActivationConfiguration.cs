using InfiniteJourney.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteJourney.Infrustructure.Persistence.Configurations;

public sealed class ModuleActivationConfiguration : IEntityTypeConfiguration<ModuleActivation>
{
    public void Configure(EntityTypeBuilder<ModuleActivation> builder)
    {
        builder.ToTable("ModuleActivations");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ModuleKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(m => new { m.TenantId, m.ModuleKey })
            .IsUnique();
    }
}
