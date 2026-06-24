namespace InfiniteJourney.Global.Shared.Api;

/// <summary>
/// Centralized API route constants — reference from controllers only.
/// </summary>
public static class ApiRoutes
{
    public const string ApiRoot = "api";

    public static class Campaigns
    {
        public const string Base = $"{ApiRoot}/campaigns";
        public const string ById = "{id:guid}";
        public const string Activate = "{id:guid}/activate";
    }

    public static class Health
    {
        public const string Base = "health";
    }
}
