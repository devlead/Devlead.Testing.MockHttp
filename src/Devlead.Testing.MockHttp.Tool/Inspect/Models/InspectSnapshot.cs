namespace Devlead.Testing.MockHttp.Tool.Inspect.Models;

internal sealed record InspectSnapshot(
    string Version,
    string ListenUrl,
    string InspectUrl,
    string ResourcesDirectory,
    bool MatchHost,
    InspectCounts Counts,
    IReadOnlyList<RouteSnapshot> Routes,
    IReadOnlyList<ActivityEntry> Activity);
