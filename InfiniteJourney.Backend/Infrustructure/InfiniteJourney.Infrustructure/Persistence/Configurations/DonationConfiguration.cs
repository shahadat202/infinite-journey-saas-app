using InfiniteJourney.Domain.Aggregates.Campaign;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfiniteJourney.Infrustructure.Persistence.Configurations;

public sealed class DonationConfiguration : IEntityTypeConfiguration<Donation>
{
    public void Configure(EntityTypeBuilder<Donation> builder)
    {
        builder.ToTable("Donations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Amount)
            .HasPrecision(18, 2);

        builder.Property(d => d.DonorEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(d => d.DonorName)
            .HasMaxLength(200);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.TransactionId)
            .HasMaxLength(255);

        builder.HasIndex(d => new { d.TenantId, d.CampaignId });
    }
}
