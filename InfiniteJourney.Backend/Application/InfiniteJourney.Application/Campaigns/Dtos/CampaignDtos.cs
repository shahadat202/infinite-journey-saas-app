using InfiniteJourney.Global.Shared.Enums;

namespace InfiniteJourney.Application.Campaigns.Dtos;

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

public sealed record CreateCampaignResultDto(Guid Id);
