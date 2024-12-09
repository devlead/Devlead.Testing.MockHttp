namespace Devlead.Testing.MockHttp;

internal class MockHttpClientFactory(Func<HttpClient> createClient) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
        => createClient();
}