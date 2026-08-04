using FluentValidation;
using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Mappings;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Exceptions;
using InfiniteJourney.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Campaigns.Commands;

public sealed class UpdateCampaignCommandHandler : ICommandHandler<UpdateCampaignCommand, CampaignDetailDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCampaignCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CampaignDetailDto> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId, cancellationToken)
            ?? throw new NotFoundException($"Campaign {request.CampaignId} was not found.");

        campaign.UpdateDetails(
            request.Title,
            request.Description,
            request.TargetAmount,
            request.CoverImageUrl,
            request.StartDate,
            request.EndDate);

        await _context.SaveChangesAsync(cancellationToken);
        return campaign.ToDetailDto();
    }
}

public sealed class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.TargetAmount).GreaterThan(0);
    }
}
