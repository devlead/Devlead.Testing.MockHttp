namespace Devlead.Testing.MockHttp.Tool.Routing.Models;

internal sealed record RouteDocument(
    RouteRequest Request,
    RouteResponse[] Responses,
    Dictionary<string, string[]>? Authorization = null
);
