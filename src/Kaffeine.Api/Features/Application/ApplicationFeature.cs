namespace Kaffeine.Api.Features.Application;

internal static class ApplicationFeature
{
    internal static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/application/status", GetStatus);
    }

    private static ApplicationStatus GetStatus() => new("Kaffeine", "development");

    private sealed record ApplicationStatus(string Name, string Status);
}
