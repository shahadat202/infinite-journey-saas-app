using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Domain.Aggregates.Campaign;

namespace InfiniteJourney.Application.Campaigns.Mappings;

public static class CampaignMappings
{
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
