using System.Collections.Concurrent;
using System.Text.Json;

namespace Devlead.Testing.MockHttp.Tool.Routing;

/// <summary>
/// Instance-scoped route table loaded from a resources directory.
/// </summary>
internal sealed partial class RouteTable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _resourcesDirectory;
    private readonly bool _matchHost;
    private readonly List<RouteEntry> _entries;
    private readonly ConcurrentDictionary<(HttpMethod Method, string AbsoluteUri), RouteEntry> _byAbsoluteUri;
    private readonly RequestActivity _activity = new();

    public string ResourcesDirectory => _resourcesDirectory;
    public bool MatchHost => _matchHost;
    public RequestActivity Activity => _activity;

    private RouteTable(
        string resourcesDirectory,
        bool matchHost,
        List<RouteEntry> entries,
        ConcurrentDictionary<(HttpMethod Method, string AbsoluteUri), RouteEntry> byAbsoluteUri)
    {
        _resourcesDirectory = resourcesDirectory;
        _matchHost = matchHost;
        _entries = entries;
        _byAbsoluteUri = byAbsoluteUri;
    }

    /// <summary>
    /// Loads <c>Routes.json</c> and response files from <paramref name="resourcesDirectory"/>.
    /// </summary>
    /// <param name="resourcesDirectory">Directory that contains <c>Routes.json</c>.</param>
    /// <param name="matchHost">When <see langword="true"/>, require the request host to match <c>AbsoluteUri</c>.</param>
    /// <returns>The loaded route table.</returns>
    public static RouteTable Load(string resourcesDirectory, bool matchHost)
    {
        var routesPath = Path.Combine(resourcesDirectory, "Routes.json");
        if (!File.Exists(routesPath))
        {
            throw new FileNotFoundException("Routes.json was not found in the resources directory.", routesPath);
        }

        using var parsed = JsonDocument.Parse(File.ReadAllText(routesPath));
        if (parsed.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Routes.json did not contain any routes.");
        }

        var entries = new List<RouteEntry>();
        var byAbsoluteUri = new ConcurrentDictionary<(HttpMethod Method, string AbsoluteUri), RouteEntry>();

        foreach (var element in parsed.RootElement.EnumerateArray())
        {
            var document = element.Deserialize<RouteDocument>(JsonOptions)
                ?? throw new InvalidOperationException("Routes.json did not contain any routes.");
            var source = element.Clone();

            foreach (var response in document.Responses)
            {
                if (!string.IsNullOrWhiteSpace(response.ContentResource))
                {
                    response.Content = ResourceFiles.GetBytes(
                        resourcesDirectory,
                        response.ContentResource,
                        document.Request.AbsoluteUri);
                }
            }

            foreach (var method in document.Request.Methods)
            {
                var httpMethod = new HttpMethod(method.Method);
                var entry = new RouteEntry(httpMethod, document, source);
                entries.Add(entry);
                byAbsoluteUri.TryAdd((httpMethod, document.Request.AbsoluteUri), entry);
            }
        }

        return new RouteTable(resourcesDirectory, matchHost, entries, byAbsoluteUri);
    }
}
