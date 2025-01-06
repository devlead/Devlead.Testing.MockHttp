using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
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
            var client = new MockHttpClient(Routes<T>.GetResponseBuilder(provider));
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

    public static IServiceCollection SimulateRetryAfter<T>(
        this IServiceCollection services,
        int rateLimitOccurrenceCount = 5,
        TimeSpan? retryAfterInterval = null
        )
    {
        long attemptCount = 0;
        retryAfterInterval ??= TimeSpan.FromSeconds(0);
        DateTimeOffset retryAfter = DateTimeOffset.MinValue;
        TimeProvider? timeProvider = null;
        bool shouldReplaceClient(
            IServiceProvider provider,
            [NotNullWhen(true)] out HttpResponseMessage? responseMessage
            )
        {
            timeProvider ??= provider.GetService<TimeProvider>() ?? TimeProvider.System;

            var utcNow = timeProvider.GetUtcNow();
            if (retryAfter <= utcNow && Interlocked.Increment(ref attemptCount) % rateLimitOccurrenceCount != 0)
            {
                responseMessage = null;
                return false;
            }

            retryAfter = retryAfter > utcNow
                            ? retryAfter
                            : utcNow.Add(retryAfterInterval.Value);

            responseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.TooManyRequests)
                                {
                                    Headers =   {
                                                    RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(retryAfter)
                                                }
                                };
            return true;
        }

        return services.AddSingleton<ShouldReplaceResponse<T>>(shouldReplaceClient);
    }

    public delegate bool ShouldReplaceResponse<T>(
        IServiceProvider provider,
        [NotNullWhen(true)]
        out HttpResponseMessage? responseMessage
        );
}
