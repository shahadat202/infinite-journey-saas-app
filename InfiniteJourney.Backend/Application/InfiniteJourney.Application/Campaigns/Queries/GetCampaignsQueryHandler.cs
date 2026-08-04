using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Mappings;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Extensions;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Application.Common.Models;
using InfiniteJourney.Domain.Aggregates.Campaign;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Queries;

public sealed class GetCampaignsQueryHandler : IQueryHandler<GetCampaignsQuery, PagedResult<CampaignListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CampaignListItemDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Campaigns.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        query = query.ApplySearch(request.Search, c => c.Title, c => c.Description);

        var sortMap = new Dictionary<string, System.Linq.Expressions.Expression<Func<Campaign, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["title"] = c => c.Title,
            ["targetamount"] = c => c.TargetAmount,
            ["raisedamount"] = c => c.RaisedAmount,
            ["status"] = c => c.Status,
            ["createdat"] = c => c.CreatedAt
        };

        query = query.ApplySort(request, sortMap, c => c.CreatedAt);

        var paged = await query.ToPagedResultAsync(request, cancellationToken);

        return new PagedResult<CampaignListItemDto>
        {
            Data = paged.Data.Select(c => c.ToListItemDto()).ToList(),
            PageIndex = paged.PageIndex,
            PageSize = paged.PageSize,
            Total = paged.Total
        };
    }
}
