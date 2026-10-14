using System.Net;

namespace Devlead.Testing.MockHttp.Tool.Routing.Models;

internal sealed record RouteResponse(
    string? ContentResource,
    string? ContentType,
    HttpStatusCode StatusCode
)
{
    public Dictionary<string, string[]> RequestHeaders { get; init; } = [];
    public Dictionary<string, string[]> ContentHeaders { get; init; } = [];
    public RouteEnableRequest[] EnableRequests { get; init; } = [];
    public RouteEnableRequest[] DisableRequests { get; init; } = [];
    internal byte[]? Content { get; set; }
}
