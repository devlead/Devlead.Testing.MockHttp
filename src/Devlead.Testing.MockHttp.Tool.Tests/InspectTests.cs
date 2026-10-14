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
        Assert.That(html, Does.Contain("exchange"));
        Assert.That(html, Does.Contain("highlightJson"));
        Assert.That(html, Does.Contain("/favicon.ico"));
        Assert.That(html, Does.Contain("https://www.nuget.org/packages/Devlead.Testing.MockHttp.Tool"));
        Assert.That(html, Does.Contain("id=\"reqRoute\""));
        Assert.That(html, Does.Contain("id=\"copyRoute\""));
    }

    [Test]
    public async Task FaviconReturnsPng()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var response = await host.Client.GetAsync("/favicon.ico");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/png"));
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.That(bytes[0], Is.EqualTo(0x89));
        Assert.That(bytes[1], Is.EqualTo((byte)'P'));
        Assert.That(bytes[2], Is.EqualTo((byte)'N'));
        Assert.That(bytes[3], Is.EqualTo((byte)'G'));
    }

    [Test]
    public async Task InspectApiAfterPostIncludesRequestBody()
    {
        await using var host = await MockHttpTestHost.StartAsync();
        using var content = new StringContent(
            """{"requestId":"dev-send","responses":[]}""",
            System.Text.Encoding.UTF8,
            "application/json");

        await host.Client.PostAsync("/messaging/send", content);
        var snapshot = await host.Client.GetFromJsonAsync<JsonElement>("/inspect/api");
        var activity = snapshot.GetProperty("activity")[0];

        Assert.That(activity.GetProperty("method").GetString(), Is.EqualTo("POST"));
        Assert.That(activity.GetProperty("requestContentType").GetString(), Does.Contain("application/json"));
        Assert.That(activity.GetProperty("requestBodyPreview").GetString(), Does.Contain("dev-send"));
        Assert.That(activity.GetProperty("requestContentLength").GetInt32(), Is.GreaterThan(0));
    }

    [Test]
    public async Task InspectApiAfterFormPostIncludesRequestBody()
    {
        await using var host = await MockHttpTestHost.StartAsync();
        using var content = new StringContent("q=a+b", System.Text.Encoding.ASCII, "application/x-www-form-urlencoded");

        await host.Client.PostAsync("/form", content);
        var snapshot = await host.Client.GetFromJsonAsync<JsonElement>("/inspect/api");
        var activity = snapshot.GetProperty("activity")[0];

        Assert.That(activity.GetProperty("requestContentType").GetString(), Does.Contain("application/x-www-form-urlencoded"));
        Assert.That(activity.GetProperty("requestBodyPreview").GetString(), Is.EqualTo("q=a+b"));
        Assert.That(activity.GetProperty("requestContentLength").GetInt32(), Is.EqualTo(5));
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
    public async Task InspectApiAfterNotFoundGeneratesRoute()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        await host.Client.GetAsync("/missing.txt");
        var snapshot = await host.Client.GetFromJsonAsync<JsonElement>("/inspect/api");
        var route = snapshot.GetProperty("activity")[0].GetProperty("route");

        Assert.That(route.GetProperty("Request").GetProperty("Methods")[0].GetProperty("Method").GetString(), Is.EqualTo("GET"));
        Assert.That(route.GetProperty("Request").GetProperty("AbsoluteUri").GetString(), Does.Contain("/missing.txt"));
        Assert.That(route.GetProperty("Responses")[0].GetProperty("StatusCode").GetInt32(), Is.EqualTo(404));
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
