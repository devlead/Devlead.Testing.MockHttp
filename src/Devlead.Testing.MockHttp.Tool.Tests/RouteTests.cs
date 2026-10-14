using System.Net.Http.Headers;

namespace Devlead.Testing.MockHttp.Tool.Tests;

public class RouteTests
{
    [Test]
    public async Task GetIndex()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var response = await host.Client.GetAsync("/index.txt");

        await Verify(response);
    }

    [Test]
    public async Task GetSecretWithoutAuthorization()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var response = await host.Client.GetAsync("/login/secret.json");

        await Verify(response);
    }

    [Test]
    public async Task GetSecretWithAuthorization()
    {
        await using var host = await MockHttpTestHost.StartAsync();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "AccessToken");

        var response = await host.Client.GetAsync("/login/secret.json");

        await Verify(response);
    }

    [Test]
    public async Task UnknownPathIsNotFound()
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var response = await host.Client.GetAsync("/missing.txt");

        await Verify(response);
    }

    [Test]
    public async Task HeaderMismatchIsBadRequest()
    {
        await using var host = await MockHttpTestHost.StartAsync(TestPaths.HeaderMismatch);

        var response = await host.Client.GetAsync("/headers.txt");

        await Verify(response);
    }

    [Test]
    public async Task HeaderMatchReturnsContent()
    {
        await using var host = await MockHttpTestHost.StartAsync(TestPaths.HeaderMismatch);
        host.Client.DefaultRequestHeaders.Add("X-Test", "yes");

        var response = await host.Client.GetAsync("/headers.txt");

        await Verify(response);
    }

    [TestCase("GET")]
    [TestCase("DELETE")]
    [TestCase("PUT")]
    [TestCase("PUT", "GET")]
    [TestCase("PUT", "DELETE")]
    [TestCase("PUT", "GET", "DELETE")]
    [TestCase("PUT", "DELETE", "GET")]
    public async Task EnableDisable(params string[] methods)
    {
        await using var host = await MockHttpTestHost.StartAsync();

        var responses = new List<HttpResponseMessage>();
        foreach (var method in methods)
        {
            responses.Add(
                await host.Client.SendAsync(
                    new HttpRequestMessage(new HttpMethod(method), "/new.txt")));
        }

        await Verify(responses).UseParameters(string.Join(",", methods));
    }

    [Test]
    public async Task MatchHostHit()
    {
        await using var host = await MockHttpTestHost.StartAsync(matchHost: true);
        host.Client.DefaultRequestHeaders.Host = "example.com";

        var response = await host.Client.GetAsync("/index.txt");

        await Verify(response);
    }

    [Test]
    public async Task MatchHostMiss()
    {
        await using var host = await MockHttpTestHost.StartAsync(matchHost: true);
        host.Client.DefaultRequestHeaders.Host = "other.com";

        var response = await host.Client.GetAsync("/index.txt");

        await Verify(response);
    }
}
