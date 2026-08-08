using InfiniteJourney.Application.Themes.Commands;
using InfiniteJourney.Application.Themes.Dtos;
using InfiniteJourney.Application.Themes.Queries;
using InfiniteJourney.Global.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteJourney.Web.Controllers;

[Route(ApiRoutes.Theme.Base)]
public sealed class ThemeController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ThemeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Get(CancellationToken cancellationToken)
        => SendOrNotFoundAsync(new GetThemeQuery(), cancellationToken);

    [HttpPut]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(ThemeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Update(
        [FromBody] UpdateThemeCommand command,
        CancellationToken cancellationToken)
        => SendAsync(command, cancellationToken);
}
