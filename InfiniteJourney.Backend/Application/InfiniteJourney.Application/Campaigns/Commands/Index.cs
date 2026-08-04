using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Common.Abstractions;

namespace InfiniteJourney.Application.Campaigns.Commands;

public sealed record ActivateCampaignCommand(Guid CampaignId) : ICommand<CampaignDetailDto>;

public sealed record CreateCampaignCommand(
    string Title,
    string Description,
    decimal TargetAmount,
    string? CoverImageUrl = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null) : ICommand<CreateCampaignResultDto>;