using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Devlead.Testing.MockHttp.Tests.Unit;

public class HttpClientTests
{
    [Test]
    public async Task GetAsync()
    {
        // Given
        var httpClient = ServiceProviderFixture.GetRequiredService<HttpClient>();

        // When
        var response = await httpClient.GetAsync("https://example.com/index.txt");

        // Then
        await Verify(response);
    }
}
