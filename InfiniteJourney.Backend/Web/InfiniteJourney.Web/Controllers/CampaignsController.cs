using InfiniteJourney.Application.Campaigns;
using InfiniteJourney.Application.Campaigns.Commands;
using InfiniteJourney.Application.Campaigns.Queries;
using InfiniteJourney.Application.Common.Models;
using InfiniteJourney.Global.Shared.Api;
using InfiniteJourney.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteJourney.Web.Controllers;

[Route(ApiRoutes.Campaigns.Base)]
[RequireModule("Campaigns")]
public sealed class CampaignsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CampaignListItemDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAll(
        [FromQuery] GetCampaignsQuery query,
        CancellationToken cancellationToken)
        => SendAsync(query, cancellationToken);


    [HttpGet(ApiRoutes.Campaigns.ById)]
    [ProducesResponseType(typeof(CampaignDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
        => SendOrNotFoundAsync(new GetCampaignByIdQuery(id), cancellationToken);


    [HttpPost]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(CreateCampaignResultDto), StatusCodes.Status201Created)]
    public Task<IActionResult> Create(
        CreateCampaignCommand command,
        CancellationToken cancellationToken)
        => SendCreatedAsync(
            command,
            cancellationToken,
            result => (nameof(GetById), new { id = result.Id }, result));


    [HttpPut(ApiRoutes.Campaigns.ById)]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(CampaignDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCampaignCommand command,
        CancellationToken cancellationToken)
        => SendAsync(command with { CampaignId = id }, cancellationToken);


    [HttpDelete(ApiRoutes.Campaigns.ById)]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCampaignCommand(id), cancellationToken);
        return NoContent();
    }


    [HttpPost(ApiRoutes.Campaigns.Activate)]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(CampaignDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
        => SendAsync(new ActivateCampaignCommand(id), cancellationToken);
}
