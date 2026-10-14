namespace Devlead.Testing.MockHttp.Tool.Routing;

/// <inheritdoc />
internal sealed partial class RouteTable
{
    private RouteEntry? FindEntry(HttpMethod method, string pathAndQuery, HostString host)
    {
        foreach (var entry in _entries)
        {
            if (!entry.Method.Equals(method))
            {
                continue;
            }

            if (!string.Equals(entry.PathAndQuery, pathAndQuery, StringComparison.Ordinal))
            {
                continue;
            }

            if (_matchHost && !HostMatches(entry.AbsoluteUri, host))
            {
                continue;
            }

            return entry;
        }

        return null;
    }

    internal static bool HostMatches(Uri absoluteUri, HostString requestHost)
    {
        if (!string.Equals(absoluteUri.Host, requestHost.Host, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var routePort = absoluteUri.IsDefaultPort
            ? SchemeDefaultPort(absoluteUri.Scheme)
            : absoluteUri.Port;

        if (requestHost.Port is null)
        {
            return routePort == SchemeDefaultPort(absoluteUri.Scheme);
        }

        return requestHost.Port == routePort;
    }

    private static int SchemeDefaultPort(string scheme) =>
        string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ? 443 : 80;
}
