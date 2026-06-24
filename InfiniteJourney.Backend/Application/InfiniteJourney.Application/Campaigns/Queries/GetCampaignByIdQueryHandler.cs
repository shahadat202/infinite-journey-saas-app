using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Mappings;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Queries;

public sealed class GetCampaignByIdQueryHandler : IQueryHandler<GetCampaignByIdQuery, CampaignDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignDetailDto?> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken);

        return campaign?.ToDetailDto();
    }
}
