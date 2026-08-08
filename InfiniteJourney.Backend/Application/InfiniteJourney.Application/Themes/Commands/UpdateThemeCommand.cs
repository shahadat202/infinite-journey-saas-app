using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Themes.Dtos;

namespace InfiniteJourney.Application.Themes.Commands;

public sealed record UpdateThemeCommand(
    string PrimaryColor,
    string SecondaryColor,
    string AccentColor,
    string FontFamily,
    bool IsDarkMode) : ICommand<ThemeDto>;
