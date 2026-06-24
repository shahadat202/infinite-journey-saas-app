using FluentValidation;
using InfiniteJourney.Application.Campaigns.Commands.ActivateCampaign;
using InfiniteJourney.Application.Campaigns.Commands.CreateCampaign;
using InfiniteJourney.Application.Campaigns.Queries.GetCampaignById;
using InfiniteJourney.Application.Campaigns.Queries.GetCampaigns;
using InfiniteJourney.Domain.Aggregates.Campaign;
using InfiniteJourney.Web.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteJourney.Web.Controllers;

[ApiController]
[Route("api/campaigns")]
[RequireModule("Campaigns")]
public sealed class CampaignsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CampaignsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCampaigns([FromQuery] CampaignStatus? status, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCampaignsQuery(status), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCampaignById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCampaignByIdQuery(id), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCampaign(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCampaignCommand(
                request.Title,
                request.Description,
                request.TargetAmount,
                request.CoverImageUrl,
                request.StartDate,
                request.EndDate),
            cancellationToken);

        return CreatedAtAction(nameof(GetCampaignById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "TenantStaff")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateCampaign(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new ActivateCampaignCommand(id), cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

public sealed record CreateCampaignRequest(
    string Title,
    string Description,
    decimal TargetAmount,
    string? CoverImageUrl = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null);
