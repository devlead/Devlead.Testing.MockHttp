using Microsoft.Extensions.DependencyInjection;
using VerifyTests.Http;

namespace Devlead.Testing.MockHttp;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMockHttpClient<T>(
        this IServiceCollection services,
        Action<HttpClient>? action = null
        )
    {
        MockHttpClient CreateClient()
        {
            var client = new MockHttpClient(Routes<T>.GetResponseBuilder());
            action?.Invoke(client);
            return client;
        }

        return services
            .AddSingleton<HttpClient>(
            _ => CreateClient()
            )
            .AddSingleton<IHttpClientFactory>(
             _ => new MockHttpClientFactory(CreateClient)
            );
    }
}
