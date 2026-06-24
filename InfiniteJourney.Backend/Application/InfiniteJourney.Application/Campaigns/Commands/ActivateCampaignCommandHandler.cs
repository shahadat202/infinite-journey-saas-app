using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Mappings;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Commands;

public sealed class ActivateCampaignCommandHandler : ICommandHandler<ActivateCampaignCommand, CampaignDetailDto>
{
    private readonly IApplicationDbContext _context;

    public ActivateCampaignCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignDetailDto> Handle(ActivateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign {request.CampaignId} was not found.");

        campaign.Activate();
        await _context.SaveChangesAsync(cancellationToken);

        return campaign.ToDetailDto();
    }
}
