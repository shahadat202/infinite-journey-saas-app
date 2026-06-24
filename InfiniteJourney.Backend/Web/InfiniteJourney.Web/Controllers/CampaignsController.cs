using InfiniteJourney.Application.Campaigns.Commands;
using InfiniteJourney.Application.Campaigns.Dtos;
using InfiniteJourney.Application.Campaigns.Queries;
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
    [ProducesResponseType(typeof(IReadOnlyList<CampaignListItemDto>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetAll([AsParameters] GetCampaignsQuery query, CancellationToken cancellationToken)
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

    [HttpPost(ApiRoutes.Campaigns.Activate)]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(typeof(CampaignDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Activate([AsParameters] ActivateCampaignRoute route, CancellationToken cancellationToken)
        => SendAsync(new ActivateCampaignCommand(route.Id), cancellationToken);
}

public sealed record GetCampaignByIdRoute(Guid Id);

public sealed record ActivateCampaignRoute(Guid Id);
