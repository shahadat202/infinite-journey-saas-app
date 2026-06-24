using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Mappings;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Domain.Aggregates.Campaign;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Queries.GetCampaigns;

public sealed record GetCampaignsQuery(CampaignStatus? Status = null) : IRequest<IReadOnlyList<CampaignListItemDto>>;

public sealed class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, IReadOnlyList<CampaignListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CampaignListItemDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Campaigns.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return campaigns.Select(c => c.ToListItemDto()).ToList();
    }
}
