using InfiniteJourney.Application.Campaigns;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Extensions;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Application.Common.Models;
using InfiniteJourney.Domain.Aggregates.Campaign;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Queries;

public sealed class GetCampaignsQueryHandler
    : IQueryHandler<GetCampaignsQuery, PagedResult<CampaignListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCampaignsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<CampaignListItemDto>> Handle(
        GetCampaignsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Campaigns.AsNoTracking();

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        query = query.ApplySearch(request.Search, c => c.Title, c => c.Description);
        query = query.ApplySort(request, SortMap, c => c.CreatedAt);

        return query.ToPagedResultAsync(request, c => c.ToListItemDto(), cancellationToken);
    }

    private static readonly Dictionary<string, System.Linq.Expressions.Expression<Func<Campaign, object>>>
        SortMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["title"]        = c => c.Title,
            ["targetamount"] = c => c.TargetAmount,
            ["raisedamount"] = c => c.RaisedAmount,
            ["status"]       = c => c.Status,
            ["createdat"]    = c => c.CreatedAt,
        };
}
