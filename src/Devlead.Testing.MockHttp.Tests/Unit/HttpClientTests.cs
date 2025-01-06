﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
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

    public class SimulateRetryAfter
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public async Task GetAsync(int rateLimitOccurrenceCount)
        {
            // Given
            var httpClient = ServiceProviderFixture
                                .GetRequiredService<HttpClient>(
                                    services => services.SimulateRetryAfter<Constants>(
                                        rateLimitOccurrenceCount: rateLimitOccurrenceCount,
                                        retryAfterInterval: TimeSpan.FromMinutes(1)
                                        )
                                );

            // When
            var responses = new List<HttpResponseMessage>();
            for (int i = 0; i < rateLimitOccurrenceCount; i++)
            {
                responses.Add(await httpClient.GetAsync("https://example.com/index.txt"));
            }

            // Then
            await Verify(responses);
        }
    }

    public class SimulateRetryAfterAndResume
    {
        [TestCase(2, 0)]
        [TestCase(2, 2)]
        public async Task GetAsync(int rateLimitOccurrenceCount, int timeMinutesAdvance)
        {
            // Given
            var (httpClient, timeProvider) = ServiceProviderFixture
                                .GetRequiredService<HttpClient, FakeTimeProvider>(
                                    services => services.SimulateRetryAfter<Constants>(
                                        rateLimitOccurrenceCount: rateLimitOccurrenceCount,
                                        retryAfterInterval: TimeSpan.FromMinutes(1)
                                        )
                                );
            TimeSpan timeAdvance = TimeSpan.FromMinutes(timeMinutesAdvance);
            var responses = new List<HttpResponseMessage>();

            // When
            for (int i = 0; i <= rateLimitOccurrenceCount * 2; i++, timeProvider.Advance(timeAdvance))
            {
                responses.Add(await httpClient.GetAsync("https://example.com/index.txt"));
            }

            // Then
            await Verify(responses);
        }
    }
}
