namespace Devlead.Testing.MockHttp.Tool.Routing;

/// <inheritdoc />
internal sealed partial class RouteTable
{
    private sealed class RouteEntry
    {
        public RouteEntry(HttpMethod method, RouteDocument document)
        {
            Method = method;
            Document = document;
            AbsoluteUri = new Uri(document.Request.AbsoluteUri, UriKind.Absolute);
            PathAndQuery = AbsoluteUri.PathAndQuery;
        }

        public HttpMethod Method { get; }
        public RouteDocument Document { get; }
        public Uri AbsoluteUri { get; }
        public string PathAndQuery { get; }
        public long Hits;
    }
}
