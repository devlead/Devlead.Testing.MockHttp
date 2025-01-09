namespace Devlead.Testing.MockHttp;

public class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseBuilder) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(
                responseBuilder(request)
            );
}
