using FluentValidation;

namespace InfiniteJourney.Application.Campaigns.Commands;

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
