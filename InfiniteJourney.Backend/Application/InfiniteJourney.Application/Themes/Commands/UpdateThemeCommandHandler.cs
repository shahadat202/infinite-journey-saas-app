using FluentValidation;
using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Exceptions;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Application.Themes.Dtos;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Themes.Commands;

public sealed class UpdateThemeCommandHandler : ICommandHandler<UpdateThemeCommand, ThemeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public UpdateThemeCommandHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ThemeDto> Handle(UpdateThemeCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            throw new InvalidOperationException("Tenant context is not resolved.");

        var theme = await _context.Themes
            .FirstOrDefaultAsync(t => t.TenantId == _tenantContext.TenantId, cancellationToken)
            ?? throw new NotFoundException("Theme not found for this tenant.");

        theme.UpdateColors(request.PrimaryColor, request.SecondaryColor, request.AccentColor);
        theme.UpdateTypography(request.FontFamily);
        theme.SetDarkMode(request.IsDarkMode);

        await _context.SaveChangesAsync(cancellationToken);

        return new ThemeDto(
            theme.Id,
            theme.PrimaryColor,
            theme.SecondaryColor,
            theme.AccentColor,
            theme.FontFamily,
            theme.IsDarkMode);
    }
}

public sealed class UpdateThemeCommandValidator : AbstractValidator<UpdateThemeCommand>
{
    public UpdateThemeCommandValidator()
    {
        RuleFor(x => x.PrimaryColor).NotEmpty().MaximumLength(20);
        RuleFor(x => x.SecondaryColor).NotEmpty().MaximumLength(20);
        RuleFor(x => x.AccentColor).NotEmpty().MaximumLength(20);
        RuleFor(x => x.FontFamily).NotEmpty().MaximumLength(100);
    }
}
