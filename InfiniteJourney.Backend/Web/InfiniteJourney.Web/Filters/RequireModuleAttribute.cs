using InfiniteJourney.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace InfiniteJourney.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireModuleAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _moduleKey;

    public RequireModuleAttribute(string moduleKey)
    {
        _moduleKey = moduleKey;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();

        if (!tenantContext.IsFeatureEnabled(_moduleKey))
        {
            context.Result = new NotFoundObjectResult(new
            {
                error = $"Module '{_moduleKey}' is not enabled for this organization."
            });
            return;
        }

        await next();
    }
}
