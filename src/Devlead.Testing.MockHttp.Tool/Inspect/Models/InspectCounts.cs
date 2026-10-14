namespace Devlead.Testing.MockHttp.Tool.Inspect.Models;

internal sealed record InspectCounts(
    int Routes,
    int Enabled,
    int Requests,
    int Unauthorized,
    int NotFound);
