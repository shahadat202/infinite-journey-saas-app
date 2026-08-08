using InfiniteJourney.Application.Common.Abstractions;
using InfiniteJourney.Application.Themes.Dtos;

namespace InfiniteJourney.Application.Themes.Queries;

public sealed record GetThemeQuery : IQuery<ThemeDto?>;
