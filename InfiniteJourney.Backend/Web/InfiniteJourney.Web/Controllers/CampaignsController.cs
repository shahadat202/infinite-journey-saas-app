using InfiniteJourney.Application.Campaigns.Commands;
using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Queries;
using InfiniteJourney.Application.Common.Models;
using InfiniteJourney.Global.Shared.Api;
using InfiniteJourney.Web.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteJourney.Web.Controllers;

[Route(ApiRoutes.Campaigns.Base)]
[RequireModule("Campaigns")]
public sealed class CampaignsController : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CampaignListItemDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAll([FromQuery] GetCampaignsQuery query, CancellationToken cancellationToken)
        => SendAsync(query, cancellationToken);

    [HttpGet(ApiRoutes.Campaigns.ById)]
    [ProducesResponseType(typeof(CampaignDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetById([AsParameters] GetCampaignByIdRoute route, CancellationToken cancellationToken)
        => SendOrNotFoundAsync(new GetCampaignByIdQuery(route.Id), cancellationToken);

    [HttpPost]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(CreateCampaignResultDto), StatusCodes.Status201Created)]
    public Task<IActionResult> Create(CreateCampaignCommand command, CancellationToken cancellationToken)
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
        [FromBody] UpdateCampaignRequest body,
        CancellationToken cancellationToken)
        => SendAsync(
            new UpdateCampaignCommand(
                id,
                body.Title,
                body.Description,
                body.TargetAmount,
                body.CoverImageUrl,
                body.StartDate,
                body.EndDate),
            cancellationToken);

    [HttpDelete(ApiRoutes.Campaigns.ById)]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await Mediator.Send(new DeleteCampaignCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost(ApiRoutes.Campaigns.Activate)]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(CampaignDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Activate([AsParameters] ActivateCampaignRoute route, CancellationToken cancellationToken)
        => SendAsync(new ActivateCampaignCommand(route.Id), cancellationToken);

    private ISender Mediator => HttpContext.RequestServices.GetRequiredService<ISender>();
}

public sealed record GetCampaignByIdRoute(Guid Id);
public sealed record ActivateCampaignRoute(Guid Id);

public sealed record UpdateCampaignRequest(
    string Title,
    string Description,
    decimal TargetAmount,
    string? CoverImageUrl = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null);
