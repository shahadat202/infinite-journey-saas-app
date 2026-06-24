using InfiniteJourney.Infrustructure.MultiTenancy;

namespace InfiniteJourney.Web.Middleware;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HashSet<string> _excludedPaths;

    public TenantResolutionMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var excluded = configuration.GetSection("MultiTenancy:ExcludedPaths").Get<string[]>()
            ?? ["/health", "/openapi", "/swagger"];

        _excludedPaths = new HashSet<string>(
            excluded.Select(NormalizePath),
            StringComparer.OrdinalIgnoreCase);
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantResolver tenantResolver,
        TenantContext tenantContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (_excludedPaths.Contains(NormalizePath(path)) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var host = context.Request.Host.Value;
        if (string.IsNullOrWhiteSpace(host))
        {
            await WriteTenantNotFound(context);
            return;
        }

        var resolution = await tenantResolver.ResolveAsync(host, context.RequestAborted);
        if (resolution is null)
        {
            await WriteTenantNotFound(context);
            return;
        }

        tenantContext.SetTenant(
            resolution.TenantId,
            resolution.TenantName,
            resolution.Subdomain,
            resolution.ConnectionString,
            resolution.EnabledModules);

        await _next(context);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        return path.EndsWith('/') && path.Length > 1
            ? path[..^1]
            : path;
    }

    private static async Task WriteTenantNotFound(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Tenant not found or inactive.",
            hint = "Access the platform using a valid tenant subdomain (e.g. org1.localhost:5000)."
        });
    }
}
