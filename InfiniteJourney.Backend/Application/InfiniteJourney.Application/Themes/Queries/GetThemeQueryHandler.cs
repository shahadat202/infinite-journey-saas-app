using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Application.Themes.Dtos;
using Microsoft.EntityFrameworkCore;

namespace InfiniteJourney.Application.Themes.Queries;

public sealed class GetThemeQueryHandler : IQueryHandler<GetThemeQuery, ThemeDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public GetThemeQueryHandler(IApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<ThemeDto?> Handle(GetThemeQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.IsResolved)
            return null;

        var theme = await _context.Themes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TenantId == _tenantContext.TenantId, cancellationToken);

        if (theme is null)
            return null;

        return new ThemeDto(
            theme.Id,
            theme.PrimaryColor,
            theme.SecondaryColor,
            theme.AccentColor,
            theme.FontFamily,
            theme.IsDarkMode);
    }
}
