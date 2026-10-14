using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Devlead.Testing.MockHttp.Tool.Tests;

internal sealed class MockHttpTestHost : IAsyncDisposable
{
    private MockHttpTestHost(WebApplication app, HttpClient client)
    {
        App = app;
        Client = client;
    }

    public WebApplication App { get; }

    public HttpClient Client { get; }

    public static async Task<MockHttpTestHost> StartAsync(string? resourcesDirectory = null, bool matchHost = false)
    {
        var table = RouteTable.Load(resourcesDirectory ?? TestPaths.LibraryResources, matchHost);
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton(table);
        builder.Services.AddSingleton(new MockHttpListenUrl("http://localhost"));
        var app = builder.Build();
        MockHttpWebHost.Map(app);
        await app.StartAsync();
        return new MockHttpTestHost(app, app.GetTestClient());
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.DisposeAsync();
    }
}

internal static class TestPaths
{
    public static string LibraryResources { get; } = Path.Combine(AppContext.BaseDirectory, "Resources");

    public static string HeaderMismatch { get; } = Path.Combine(AppContext.BaseDirectory, "Fixtures", "HeaderMismatch");
}
