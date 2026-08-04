using InfiniteJourney.Application.Common.Interfaces;
using InfiniteJourney.Application.Files.Commands;
using InfiniteJourney.Global.Shared.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteJourney.Web.Controllers;

[Route(ApiRoutes.Files.Base)]
public sealed class FilesController : ApiControllerBase
{
    [HttpPost(ApiRoutes.Files.Upload)]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(UploadFileResultDto), StatusCodes.Status200OK)]
    public Task<IActionResult> Upload(UploadFileCommand command, CancellationToken cancellationToken)
        => SendAsync(command, cancellationToken);
}
