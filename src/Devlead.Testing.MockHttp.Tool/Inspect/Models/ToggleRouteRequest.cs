namespace Devlead.Testing.MockHttp.Tool.Inspect.Models;

internal sealed record ToggleRouteRequest(string Method, string AbsoluteUri, bool? Enabled = null);
