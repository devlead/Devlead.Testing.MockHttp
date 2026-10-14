namespace Devlead.Testing.MockHttp.Tool.Hosting;

/// <summary>
/// Builds and maps the mock HTTP application.
/// </summary>
internal static partial class MockHttpWebHost
{
    /// <summary>
    /// Creates a Kestrel-backed builder that listens on <paramref name="listenUrl"/>.
    /// </summary>
    /// <param name="table">The in-memory route table.</param>
    /// <param name="listenUrl">The listen authority, for example <c>http://127.0.0.1:5080</c>.</param>
    /// <returns>The configured web application builder.</returns>
    public static WebApplicationBuilder CreateBuilder(RouteTable table, string listenUrl)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(listenUrl);
        builder.Services.AddSingleton(table);
        builder.Services.AddSingleton(new MockHttpListenUrl(listenUrl));
        return builder;
    }

    /// <summary>
    /// Maps inspect endpoints, then the mock HTTP fallback.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void Map(WebApplication app)
    {
        app.MapGet("/inspect", InspectPage);
        app.MapGet("/inspect/api", InspectState);
        app.MapPost("/inspect/api/routes/toggle", InspectToggle);
        app.MapDelete("/inspect/api/activity", InspectClearActivity);

        var table = app.Services.GetRequiredService<RouteTable>();
        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments("/inspect"),
            branch => branch.Run(table.InvokeAsync));
    }
}
