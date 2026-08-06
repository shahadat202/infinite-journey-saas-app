using InfiniteJourney.Domain.Aggregates.Campaign;
using InfiniteJourney.Global.Shared.Enums;

namespace InfiniteJourney.Application.Campaigns;

// =============================================================================
// DTOs
// All data-transfer shapes for the Campaigns feature live here.
// Handlers reference this file; nothing else needed.
// =============================================================================

/// <summary>Lightweight row used in list/grid views.</summary>
public sealed record CampaignListItemDto(
    Guid Id,
    string Title,
    string Description,
    decimal TargetAmount,
    decimal RaisedAmount,
    CampaignStatus Status,
    string? CoverImageUrl,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate);

/// <summary>Full detail view, including computed progress and audit timestamp.</summary>
public sealed record CampaignDetailDto(
    Guid Id,
    string Title,
    string Description,
    decimal TargetAmount,
    decimal RaisedAmount,
    CampaignStatus Status,
    string? CoverImageUrl,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    DateTimeOffset CreatedAt,
    decimal ProgressPercent);

/// <summary>Minimal response returned after a successful create.</summary>
public sealed record CreateCampaignResultDto(Guid Id);

// =============================================================================
// Mappings
// Static extension methods — no AutoMapper overhead, no hidden conventions.
// Add a new mapping here when you add a new DTO above.
// =============================================================================

public static class CampaignMappings
{
    /// <summary>Maps a Campaign entity to the grid list-item shape.</summary>
    public static CampaignListItemDto ToListItemDto(this Campaign campaign) =>
        new(
            campaign.Id,
            campaign.Title,
            campaign.Description,
            campaign.TargetAmount,
            campaign.RaisedAmount,
            campaign.Status,
            campaign.CoverImageUrl,
            campaign.StartDate,
            campaign.EndDate);

    /// <summary>
    /// Maps a Campaign entity to the full detail shape.
    /// Computes <c>ProgressPercent</c> here so it never leaks into domain logic.
    /// </summary>
    public static CampaignDetailDto ToDetailDto(this Campaign campaign)
    {
        var progress = campaign.TargetAmount > 0
            ? Math.Round(campaign.RaisedAmount / campaign.TargetAmount * 100m, 2)
            : 0m;

        return new CampaignDetailDto(
            campaign.Id,
            campaign.Title,
            campaign.Description,
            campaign.TargetAmount,
            campaign.RaisedAmount,
            campaign.Status,
            campaign.CoverImageUrl,
            campaign.StartDate,
            campaign.EndDate,
            campaign.CreatedAt,
            progress);
    }
}
