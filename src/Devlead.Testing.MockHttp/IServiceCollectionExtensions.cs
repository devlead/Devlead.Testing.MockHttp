using Microsoft.Extensions.DependencyInjection;
using VerifyTests.Http;

namespace Devlead.Testing.MockHttp;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddMockHttpClient<T>(
        this IServiceCollection services
        )
    {
        static MockHttpClient CreateClient(IServiceProvider provider)
        {
            var client = new MockHttpClient(Routes<T>.GetResponseBuilder());
            foreach(var service in provider.GetServices<ConfigureHttpClient<T>>())
            {
                service?.Invoke(client);
            }
            return client;
        }

        return services
            .AddSingleton<HttpClient>(CreateClient)
            .AddSingleton<IHttpClientFactory>(
             provider => new MockHttpClientFactory(()=>CreateClient(provider))
            );
    }

    public static IServiceCollection ConfigureMockHttpClient<T>(
        this IServiceCollection services,
        ConfigureHttpClient<T> configure
        )
        => services.AddSingleton(configure);

    public delegate void ConfigureHttpClient<T>(HttpClient client);
}
