using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Common.Abstractions;

namespace InfiniteJourney.Application.Campaigns.Commands;

public sealed record ActivateCampaignCommand(Guid CampaignId) : ICommand<CampaignDetailDto>;
