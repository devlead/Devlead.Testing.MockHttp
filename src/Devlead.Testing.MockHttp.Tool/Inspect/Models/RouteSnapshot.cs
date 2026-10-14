namespace Devlead.Testing.MockHttp.Tool.Inspect.Models;

internal sealed record RouteSnapshot(
    string Method,
    string AbsoluteUri,
    string Host,
    string PathAndQuery,
    bool Enabled,
    long Hits);
