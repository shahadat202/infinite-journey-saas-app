using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Global.Shared.Enums;

namespace InfiniteJourney.Application.Campaigns.Queries;

public sealed record GetCampaignsQuery(CampaignStatus? Status = null) : IQuery<IReadOnlyList<CampaignListItemDto>>;
