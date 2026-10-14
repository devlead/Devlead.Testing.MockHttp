namespace Devlead.Testing.MockHttp.Tool.Routing.Models;

internal sealed record RouteRequest(
    RouteMethod[] Methods,
    string AbsoluteUri
)
{
    public bool Disabled { get; set; }
}
