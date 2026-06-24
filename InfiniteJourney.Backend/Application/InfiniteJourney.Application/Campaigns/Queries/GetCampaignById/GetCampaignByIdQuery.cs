using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Mappings;
using InfiniteJourney.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Queries.GetCampaignById;

public sealed record GetCampaignByIdQuery(Guid CampaignId) : IRequest<CampaignDetailDto?>;

public sealed class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, CampaignDetailDto?>
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
