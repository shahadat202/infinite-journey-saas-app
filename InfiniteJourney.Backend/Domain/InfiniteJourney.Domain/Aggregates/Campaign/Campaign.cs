using System;
using System.Collections.Generic;
using InfiniteJourney.Domain.Common;

namespace InfiniteJourney.Domain.Aggregates.Campaign;

public class Campaign : BaseTenantEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal TargetAmount { get; private set; }
    public decimal RaisedAmount { get; private set; }
    public CampaignStatus Status { get; private set; } = CampaignStatus.Draft;
    public string? CoverImageUrl { get; private set; }
    public DateTimeOffset? StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }

    private readonly List<Donation> _donations = new();
    public virtual IReadOnlyCollection<Donation> Donations => _donations.AsReadOnly();

    private Campaign() { } // Required for EF Core

    public static Campaign Create(Guid tenantId, string title, string description, decimal targetAmount, string? coverImageUrl = null, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Campaign title cannot be empty.", nameof(title));
        if (targetAmount <= 0)
            throw new ArgumentException("Target amount must be greater than zero.", nameof(targetAmount));

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Title = title.Trim(),
            Description = description.Trim(),
            TargetAmount = targetAmount,
            RaisedAmount = 0,
            Status = CampaignStatus.Draft,
            CoverImageUrl = coverImageUrl,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTimeOffset.UtcNow
        };

        campaign.AddDomainEvent(new CampaignCreatedEvent(campaign.Id, campaign.TenantId, campaign.Title));
        return campaign;
    }

    public void UpdateDetails(string title, string description, decimal targetAmount, string? coverImageUrl, DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Campaign title cannot be empty.", nameof(title));
        if (targetAmount <= 0)
            throw new ArgumentException("Target amount must be greater than zero.", nameof(targetAmount));

        Title = title.Trim();
        Description = description.Trim();
        TargetAmount = targetAmount;
        CoverImageUrl = coverImageUrl;
        StartDate = startDate;
        EndDate = endDate;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void Activate()
    {
        if (Status == CampaignStatus.Active) return;
        Status = CampaignStatus.Active;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void End()
    {
        if (Status == CampaignStatus.Ended) return;
        Status = CampaignStatus.Ended;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void RecordDonation(Guid donationId, decimal amount, string donorEmail)
    {
        if (Status != CampaignStatus.Active)
            throw new InvalidOperationException("Cannot record donations on a campaign that is not active.");
        if (amount <= 0)
            throw new ArgumentException("Donation amount must be positive.", nameof(amount));

        decimal oldRaised = RaisedAmount;
        RaisedAmount += amount;
        LastModifiedAt = DateTimeOffset.UtcNow;

        AddDomainEvent(new CampaignProgressUpdatedEvent(Id, TenantId, RaisedAmount, TargetAmount));

        if (oldRaised < TargetAmount && RaisedAmount >= TargetAmount)
        {
            AddDomainEvent(new CampaignGoalReachedEvent(Id, TenantId, RaisedAmount, TargetAmount));
        }
    }
}

public record CampaignCreatedEvent(Guid CampaignId, Guid TenantId, string Title) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public record CampaignProgressUpdatedEvent(Guid CampaignId, Guid TenantId, decimal RaisedAmount, decimal TargetAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public record CampaignGoalReachedEvent(Guid CampaignId, Guid TenantId, decimal RaisedAmount, decimal TargetAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
