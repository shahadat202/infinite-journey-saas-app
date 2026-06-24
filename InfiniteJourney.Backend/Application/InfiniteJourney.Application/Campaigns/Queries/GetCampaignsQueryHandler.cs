using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Mappings;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Queries;

public sealed class GetCampaignsQueryHandler : IQueryHandler<GetCampaignsQuery, IReadOnlyList<CampaignListItemDto>>
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
