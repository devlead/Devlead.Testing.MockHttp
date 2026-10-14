using System.Security.Cryptography;

namespace Devlead.Testing.MockHttp.Tool.Routing;

/// <inheritdoc />
internal sealed partial class RouteTable
{
    public async Task InvokeAsync(HttpContext context)
    {
        var requestHeaders = CaptureHeaders(context.Request.Headers);
        var method = new HttpMethod(context.Request.Method);
        var pathAndQuery = $"{context.Request.Path}{context.Request.QueryString}";
        var host = context.Request.Host.Value ?? string.Empty;
        var matched = "-";
        var enableDisable = false;
        byte[]? content = null;
        string? contentType = null;

        try
        {
            var entry = FindEntry(method, pathAndQuery, context.Request.Host);
            if (entry is null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            matched = entry.Document.Request.AbsoluteUri;

            if (entry.Document.Request.Disabled)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            Interlocked.Increment(ref entry.Hits);

            if (entry.Document.Authorization is { } authorization)
            {
                foreach (var header in authorization)
                {
                    if (!TryGetHeaderValues(context.Request, header.Key, out var values)
                        || !values.All(value => header.Value.Contains(value)))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }
                }
            }

            var response = entry.Document.Responses.FirstOrDefault(
                candidate => candidate.RequestHeaders.All(
                    header =>
                        TryGetHeaderValues(context.Request, header.Key, out var values)
                        && values.All(value => header.Value.Contains(value))));

            if (response is null)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            foreach (var enableRequest in response.EnableRequests)
            {
                if (_byAbsoluteUri.TryGetValue((enableRequest.Method, enableRequest.AbsoluteUri), out var target))
                {
                    target.Document.Request.Disabled = false;
                    enableDisable = true;
                }
            }

            foreach (var disableRequest in response.DisableRequests)
            {
                if (_byAbsoluteUri.TryGetValue((disableRequest.Method, disableRequest.AbsoluteUri), out var target))
                {
                    target.Document.Request.Disabled = true;
                    enableDisable = true;
                }
            }

            context.Response.StatusCode = (int)response.StatusCode;

            foreach (var (key, value) in response.ContentHeaders)
            {
                context.Response.Headers[key] = value;
            }

            if (!string.IsNullOrWhiteSpace(response.ContentResource))
            {
                content = response.Content
                    ?? throw new InvalidOperationException(
                        $"Response content for {response.ContentResource} was not loaded.");

                if (!string.IsNullOrWhiteSpace(response.ContentType))
                {
                    context.Response.ContentType = response.ContentType;
                    contentType = response.ContentType;
                }

                context.Response.Headers.ContentMD5 = Convert.ToBase64String(MD5.HashData(content));
                await context.Response.Body.WriteAsync(content).ConfigureAwait(false);
            }
        }
        finally
        {
            contentType ??= context.Response.ContentType;
            _activity.Add(
                new ActivityEntry(
                    Guid.NewGuid().ToString("n")[..12],
                    DateTimeOffset.UtcNow,
                    method.Method,
                    pathAndQuery,
                    host,
                    context.Response.StatusCode,
                    matched,
                    enableDisable,
                    requestHeaders,
                    CaptureHeaders(context.Response.Headers),
                    contentType,
                    content?.Length ?? 0,
                    BodyPreview(content, contentType)));
        }
    }
}
