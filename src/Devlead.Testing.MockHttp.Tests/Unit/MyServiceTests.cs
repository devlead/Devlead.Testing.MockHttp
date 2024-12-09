using Devlead.Testing.MockHttp.Tests.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Devlead.Testing.MockHttp.Tests.Unit;

public class MyServiceTests
{
    [Test]
    public async Task GetData()
    {
        // Given
        var serviceCollection = new ServiceCollection()
            .AddSingleton<MyService>()
            .AddMockHttpClient<Constants>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myService = serviceProvider.GetRequiredService<MyService>();

        // When
        var result = await myService.GetData();

        // Then
        await Verify(result);
    }

    [Test]
    public async Task GetUnauthorizedSecret()
    {
        // Given
        var serviceCollection = new ServiceCollection()
            .AddSingleton<MyService>()
            .AddMockHttpClient<Constants>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myService = serviceProvider.GetRequiredService<MyService>();

        // When 
        var result = Assert.CatchAsync<HttpRequestException>(myService.GetSecret);

        // Then
        await Verify(result);
    }

    [Test]
    public async Task GetSecret()
    {
        // Given
        var serviceCollection = new ServiceCollection()
            .AddSingleton<MyService>()
            .AddMockHttpClient<Constants>(
                client => client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    "AccessToken"
                    )
            );
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var myService = serviceProvider.GetRequiredService<MyService>();

        // When
        var result = await myService.GetSecret();

        // Then
        await Verify(result);
    }
}