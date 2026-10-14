using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Devlead.Testing.MockHttp.Tool.Tests;

public class InspectTests
{
    [Test]
    public async Task InspectPageReturnsHtml()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var response = await host.Client.GetAsync("/inspect");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"));
        var html = await response.Content.ReadAsStringAsync();
        Assert.That(html, Does.Contain("MockHttp inspect"));
        Assert.That(html, Does.Contain("Request headers"));
        Assert.That(html, Does.Contain("Response headers"));
    }

    [Test]
    public async Task InspectApiAfterGetIncludesHitAndHeaders()
    {
        await using var host = await MockHttpTestHost.StartAsync();
        host.Client.DefaultRequestHeaders.Add("X-Test", "inspect");

        await host.Client.GetAsync("/index.txt");
        var json = await host.Client.GetStringAsync("/inspect/api");

        await VerifyJson(json)
            .ScrubMember("version")
            .ScrubMember("resourcesDirectory")
            .ScrubMember("id")
            .ScrubMember("utc");
    }

    [Test]
    public async Task DisabledRouteGetDoesNotIncrementHits()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var response = await host.Client.GetAsync("/new.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

        var snapshot = await host.Client.GetFromJsonAsync<JsonElement>("/inspect/api");
        var route = snapshot.GetProperty("routes").EnumerateArray().Single(item =>
            item.GetProperty("method").GetString() == "GET"
            && item.GetProperty("pathAndQuery").GetString() == "/new.txt");
        Assert.That(route.GetProperty("hits").GetInt64(), Is.EqualTo(0));
    }

    [Test]
    public async Task ToggleDisableThenGetIsNotFound()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var toggle = await host.Client.PostAsJsonAsync(
            "/inspect/api/routes/toggle",
            new { method = "GET", absoluteUri = "https://example.com/index.txt", enabled = false });
        Assert.That(toggle.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var response = await host.Client.GetAsync("/index.txt");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ClearActivityRemovesHits()
    {
        await using var host = await MockHttpTestHost.StartAsync();
        await host.Client.GetAsync("/index.txt");

        var clear = await host.Client.DeleteAsync("/inspect/api/activity");
        Assert.That(clear.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var snapshot = await host.Client.GetFromJsonAsync<JsonElement>("/inspect/api");
        Assert.That(snapshot.GetProperty("activity").GetArrayLength(), Is.EqualTo(0));
        Assert.That(snapshot.GetProperty("counts").GetProperty("requests").GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public async Task InspectIsNotMockedAsNotFound()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var response = await host.Client.GetAsync("/inspect");

        Assert.That(response.StatusCode, Is.Not.EqualTo(HttpStatusCode.NotFound));
    }
}
