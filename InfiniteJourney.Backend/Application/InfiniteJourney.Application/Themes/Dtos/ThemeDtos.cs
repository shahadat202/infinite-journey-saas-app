namespace InfiniteJourney.Application.Themes.Dtos;

public sealed record ThemeDto(
    Guid Id,
    string PrimaryColor,
    string SecondaryColor,
    string AccentColor,
    string FontFamily,
    bool IsDarkMode);

public sealed record UpdateThemeRequest(
    string PrimaryColor,
    string SecondaryColor,
    string AccentColor,
    string FontFamily,
    bool IsDarkMode);
