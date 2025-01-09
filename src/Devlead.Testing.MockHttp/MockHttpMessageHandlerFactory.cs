using System.Collections.Concurrent;

namespace Devlead.Testing.MockHttp;

public class MockHttpMessageHandlerFactory(Func<HttpRequestMessage, HttpResponseMessage> responseBuilder) : IHttpMessageHandlerFactory
{
    private ConcurrentDictionary<string, MockHttpMessageHandler> HttpMessageHandlers { get; } = [];

    public HttpMessageHandler CreateHandler(string name)
        => HttpMessageHandlers.GetOrAdd(
            name,
            _ => new(responseBuilder)
            );
}
