using InfiniteJourney.Domain.Aggregates.Campaign;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteJourney.Infrustructure.Persistence.Configurations;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaigns");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(4000);

        builder.Property(c => c.TargetAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.RaisedAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(c => c.CoverImageUrl)
            .HasMaxLength(500);

        builder.HasIndex(c => c.TenantId);

        builder.HasMany(c => c.Donations)
            .WithOne()
            .HasForeignKey(d => d.CampaignId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
