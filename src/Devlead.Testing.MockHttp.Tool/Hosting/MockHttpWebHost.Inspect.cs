using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlead.Testing.MockHttp.Tool.Hosting;

/// <inheritdoc />
internal static partial class MockHttpWebHost
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly string ApplicationVersion =
        typeof(MockHttpWebHost).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            is { Length: > 0 } informational
            ? informational.Split('+', 2)[0]
            : typeof(MockHttpWebHost).Assembly.GetName().Version?.ToString() ?? string.Empty;

    private static IResult InspectPage()
    {
        using var stream = typeof(MockHttpWebHost).Assembly
            .GetManifestResourceStream("Devlead.Testing.MockHttp.Tool.Inspect.inspect.html")
            ?? throw new InvalidOperationException("The inspect page resource is missing.");
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return Results.Content(reader.ReadToEnd(), "text/html", Encoding.UTF8);
    }

    private static IResult InspectState(RouteTable table, MockHttpListenUrl listen) =>
        Results.Json(table.Snapshot(listen.Value, ApplicationVersion), JsonOptions);

    private static IResult InspectToggle(ToggleRouteRequest body, RouteTable table)
    {
        if (string.IsNullOrWhiteSpace(body.Method) || string.IsNullOrWhiteSpace(body.AbsoluteUri))
        {
            return Results.BadRequest();
        }

        return table.TryToggle(body.Method, body.AbsoluteUri, body.Enabled)
            ? Results.NoContent()
            : Results.NotFound();
    }

    private static IResult InspectClearActivity(RouteTable table)
    {
        table.Activity.Clear();
        return Results.NoContent();
    }
}
