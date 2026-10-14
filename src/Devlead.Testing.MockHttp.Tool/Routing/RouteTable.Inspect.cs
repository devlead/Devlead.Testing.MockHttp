namespace Devlead.Testing.MockHttp.Tool.Routing;

/// <inheritdoc />
internal sealed partial class RouteTable
{
    public InspectSnapshot Snapshot(string listenUrl, string version)
    {
        var routes = _entries
            .Select(entry => new RouteSnapshot(
                entry.Method.Method,
                entry.Document.Request.AbsoluteUri,
                entry.AbsoluteUri.Host,
                entry.PathAndQuery,
                !entry.Document.Request.Disabled,
                Interlocked.Read(ref entry.Hits)))
            .ToArray();

        return new InspectSnapshot(
            version,
            listenUrl,
            $"{listenUrl.TrimEnd('/')}/inspect",
            _resourcesDirectory,
            _matchHost,
            new InspectCounts(
                routes.Length,
                routes.Count(route => route.Enabled),
                _activity.Requests,
                _activity.Unauthorized,
                _activity.NotFound),
            routes,
            _activity.Snapshot());
    }

    public bool TryToggle(string method, string absoluteUri, bool? enabled)
    {
        if (!_byAbsoluteUri.TryGetValue((new HttpMethod(method), absoluteUri), out var entry))
        {
            return false;
        }

        entry.Document.Request.Disabled = enabled is { } value ? !value : !entry.Document.Request.Disabled;
        return true;
    }
}
