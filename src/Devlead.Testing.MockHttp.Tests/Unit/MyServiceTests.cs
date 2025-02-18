using Devlead.Testing.MockHttp.Tests.Services;

namespace Devlead.Testing.MockHttp.Tests.Unit;

public class MyServiceTests
{
    [Test]
    public async Task GetData()
    {
        // Given
        var myService = ServiceProviderFixture.GetRequiredService<MyService>();

        // When
        var result = await myService.GetData();

        // Then
        await Verify(result);
    }

    [Test]
    public async Task GetUnauthorizedSecret()
    {
        // Given
        var myService = ServiceProviderFixture.GetRequiredService<MyService>();

        // When 
        var result = Assert.CatchAsync<HttpRequestException>(myService.GetSecret);

        // Then
        await Verify(result);
    }

    [Test]
    public async Task GetSecret()
    {
        // Given

        var myService = ServiceProviderFixture.GetRequiredService<MyService>(
                            configure => configure.ConfigureMockHttpClient<Constants>(
                                            client => client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                                                "Bearer",
                                                "AccessToken"
                                                )
                                        )
                            );

        // When
        var result = await myService.GetSecret();

        // Then
        await Verify(result);
    }
    [Test]
    public async Task GetServiceName()
    {
        // Given
        var myService = ServiceProviderFixture.GetRequiredService<MyService>();

        // When
        var result = myService.GetServiceName();

        // Then
        await Verify(result);
    }
}