using InfiniteJourney.Application.Campaigns;
using InfiniteJourney.Application.Common.Abstractions;

namespace InfiniteJourney.Application.Campaigns.Commands;

// ---------------------------------------------------------------------------
// Create
// The controller binds the request body directly as this command — no mapping.
// Validator: CreateCampaignCommandHandler.cs
// ---------------------------------------------------------------------------

public sealed record CreateCampaignCommand(
    string Title,
    string Description,
    decimal TargetAmount,
    string? CoverImageUrl = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null) : ICommand<CreateCampaignResultDto>;

// ---------------------------------------------------------------------------
// Update
// The controller binds the request body directly as this command, then sets
// CampaignId from the route — no separate body DTO, no field spreading.
// Validator: UpdateCampaignCommandHandler.cs
// ---------------------------------------------------------------------------

public sealed record UpdateCampaignCommand(
    string Title,
    string Description,
    decimal TargetAmount,
    string? CoverImageUrl = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null) : ICommand<CampaignDetailDto>
{
    /// <summary>
    /// Set by the controller from the route parameter after model binding.
    /// Not part of the JSON body — the client never sends this.
    /// </summary>
    public Guid CampaignId { get; init; }
}

// ---------------------------------------------------------------------------
// Delete
// ---------------------------------------------------------------------------

public sealed record DeleteCampaignCommand(Guid CampaignId) : ICommand<bool>;

// ---------------------------------------------------------------------------
// Activate
// ---------------------------------------------------------------------------

public sealed record ActivateCampaignCommand(Guid CampaignId) : ICommand<CampaignDetailDto>;
