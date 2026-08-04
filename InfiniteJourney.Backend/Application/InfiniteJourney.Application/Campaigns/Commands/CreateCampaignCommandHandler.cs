using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Domain.Aggregates.Campaign;
using FluentValidation;

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

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(500)
            .When(x => x.CoverImageUrl is not null);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
