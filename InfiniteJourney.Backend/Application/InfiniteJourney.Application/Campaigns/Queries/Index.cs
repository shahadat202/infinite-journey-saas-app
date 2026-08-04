using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Global.Shared.Enums;

namespace InfiniteJourney.Application.Campaigns.Queries;

public sealed record GetCampaignByIdQuery(Guid CampaignId) : IQuery<CampaignDetailDto?>;

public sealed class GetCampaignsQuery : Common.Models.GridQuery, IQuery<Common.Models.PagedResult<CampaignListItemDto>>
{
    public CampaignStatus? Status { get; set; }
}
