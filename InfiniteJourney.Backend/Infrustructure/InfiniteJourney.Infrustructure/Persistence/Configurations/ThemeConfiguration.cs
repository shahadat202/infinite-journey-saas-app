using InfiniteJourney.Domain.Aggregates.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteJourney.Infrustructure.Persistence.Configurations;

public sealed class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {
        builder.ToTable("Themes");

        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.TenantId)
            .IsUnique();

        builder.Property(t => t.PrimaryColor).HasMaxLength(20).IsRequired();
        builder.Property(t => t.SecondaryColor).HasMaxLength(20).IsRequired();
        builder.Property(t => t.AccentColor).HasMaxLength(20).IsRequired();
        builder.Property(t => t.FontFamily).HasMaxLength(100).IsRequired();
    }
}
