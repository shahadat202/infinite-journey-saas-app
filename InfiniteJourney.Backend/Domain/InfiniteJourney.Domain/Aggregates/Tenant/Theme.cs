using System;
using InfiniteJourney.Domain.Common;

namespace InfiniteJourney.Domain.Aggregates.Tenant;

public class Theme : BaseTenantEntity
{
    public string PrimaryColor { get; private set; } = "#1E3A8A";
    public string SecondaryColor { get; private set; } = "#10B981";
    public string AccentColor { get; private set; } = "#F59E0B";
    public string FontFamily { get; private set; } = "Inter, sans-serif";
    public bool IsDarkMode { get; private set; }

    private Theme() { } // Required for EF Core

    public static Theme CreateDefault(Guid tenantId)
    {
        return new Theme
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PrimaryColor = "#1E3A8A",
            SecondaryColor = "#10B981",
            AccentColor = "#F59E0B",
            FontFamily = "Inter, sans-serif",
            IsDarkMode = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateColors(string primary, string secondary, string accent)
    {
        PrimaryColor = primary;
        SecondaryColor = secondary;
        AccentColor = accent;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateTypography(string fontFamily)
    {
        FontFamily = fontFamily;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }

    public void SetDarkMode(bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        LastModifiedAt = DateTimeOffset.UtcNow;
    }
}
