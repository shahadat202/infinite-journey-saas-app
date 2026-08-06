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

    public async Task<bool> Handle(
        DeleteCampaignCommand request,
        CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken)
            ?? throw new NotFoundException($"Campaign {request.CampaignId} was not found.");

        // Domain guard — throws if the campaign has received donations.
        // We translate to ConflictException (HTTP 409) so the client knows
        // this is a state conflict, not a generic business rule violation.
        if (campaign.RaisedAmount > 0)
            throw new ConflictException(
                "Cannot delete a campaign that has received donations.");

        if (!string.IsNullOrWhiteSpace(campaign.CoverImageUrl))
            await _fileStorage.DeleteAsync(campaign.CoverImageUrl, cancellationToken);

        _context.Campaigns.Remove(campaign);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
