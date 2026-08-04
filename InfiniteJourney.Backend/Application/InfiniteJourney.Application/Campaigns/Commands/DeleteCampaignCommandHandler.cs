using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Exceptions;
using InfiniteJourney.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Commands;

public sealed class DeleteCampaignCommandHandler : ICommandHandler<DeleteCampaignCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorage;

    public DeleteCampaignCommandHandler(IApplicationDbContext context, IFileStorageService fileStorage)
    {
        _context = context;
        _fileStorage = fileStorage;
    }

    public async Task<bool> Handle(DeleteCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken)
            ?? throw new NotFoundException($"Campaign {request.CampaignId} was not found.");

        try
        {
            campaign.EnsureCanDelete();
        }
        catch (InvalidOperationException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }

        if (!string.IsNullOrWhiteSpace(campaign.CoverImageUrl))
            await _fileStorage.DeleteAsync(campaign.CoverImageUrl, cancellationToken);

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
