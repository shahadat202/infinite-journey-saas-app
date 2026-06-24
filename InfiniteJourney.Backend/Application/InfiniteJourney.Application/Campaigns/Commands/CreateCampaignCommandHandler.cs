using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Domain.Aggregates.Campaign;

namespace InfiniteJourney.Application.Campaigns.Commands;

public sealed class CreateCampaignCommandHandler : ICommandHandler<CreateCampaignCommand, CreateCampaignResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public CreateCampaignCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<CreateCampaignResultDto> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            throw new InvalidOperationException("Tenant context is not resolved.");

        var campaign = Campaign.Create(
            _tenantContext.TenantId,
            request.Title,
            request.Description,
            request.TargetAmount,
            request.CoverImageUrl,
            request.StartDate,
            request.EndDate);

        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateCampaignResultDto(campaign.Id);
    }
}
