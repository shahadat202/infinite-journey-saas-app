using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Common.Abstractions;

namespace InfiniteJourney.Application.Campaigns.Queries;

public sealed record GetCampaignByIdQuery(Guid CampaignId) : IQuery<CampaignDetailDto?>;
