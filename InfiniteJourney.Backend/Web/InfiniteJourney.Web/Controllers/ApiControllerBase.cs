using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteJourney.Web.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender Mediator => HttpContext.RequestServices.GetRequiredService<ISender>();

    protected Task<IActionResult> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken)
        => SendAsync(request, cancellationToken, result => Ok(result));

    protected async Task<IActionResult> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken,
        Func<TResponse, IActionResult> onSuccess)
    {
        var result = await Mediator.Send(request, cancellationToken);
        return onSuccess(result);
    }

    protected async Task<IActionResult> SendOrNotFoundAsync<TResponse>(
        IRequest<TResponse?> request,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        var result = await Mediator.Send(request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    protected async Task<IActionResult> SendCreatedAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken,
        Func<TResponse, (string actionName, object routeValues, object body)> createdResultFactory)
    {
        var result = await Mediator.Send(request, cancellationToken);
        var (actionName, routeValues, body) = createdResultFactory(result);
        return CreatedAtAction(actionName, routeValues, body);
    }
}
