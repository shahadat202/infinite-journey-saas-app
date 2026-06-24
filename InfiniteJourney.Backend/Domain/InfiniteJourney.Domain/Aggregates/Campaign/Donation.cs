using System;
using InfiniteJourney.Domain.Common;

namespace InfiniteJourney.Domain.Aggregates.Campaign;

public class Donation : BaseTenantEntity
{
    public Guid CampaignId { get; private set; }
    public decimal Amount { get; private set; }
    public string DonorEmail { get; private set; } = string.Empty;
    public string DonorName { get; private set; } = string.Empty;
    public DonationStatus Status { get; private set; } = DonationStatus.Pending;
    public string? TransactionId { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

    private Donation() { } // Required for EF Core

    public static Donation Create(Guid tenantId, Guid campaignId, decimal amount, string donorEmail, string donorName)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (campaignId == Guid.Empty)
            throw new ArgumentException("Campaign ID cannot be empty.", nameof(campaignId));
        if (amount <= 0)
            throw new ArgumentException("Donation amount must be positive.", nameof(amount));
        if (string.IsNullOrWhiteSpace(donorEmail))
            throw new ArgumentException("Donor email cannot be empty.", nameof(donorEmail));

        return new Donation
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CampaignId = campaignId,
            Amount = amount,
            DonorEmail = donorEmail.ToLowerInvariant().Trim(),
            DonorName = donorName.Trim(),
            Status = DonationStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Complete(string transactionId)
    {
        if (Status == DonationStatus.Completed) return;

        Status = DonationStatus.Completed;
        TransactionId = transactionId;
        ProcessedAt = DateTimeOffset.UtcNow;
        LastModifiedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new DonationCompletedEvent(Id, TenantId, CampaignId, Amount, DonorEmail));
    }

    public void Fail()
    {
        if (Status == DonationStatus.Completed)
            throw new InvalidOperationException("Cannot fail a donation that has already been completed.");

        Status = DonationStatus.Failed;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
}

public record DonationCompletedEvent(Guid DonationId, Guid TenantId, Guid CampaignId, decimal Amount, string DonorEmail) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
