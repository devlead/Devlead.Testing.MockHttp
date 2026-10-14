using System.Text;
using System.Text.Json;

namespace Devlead.Testing.MockHttp.Tool.Routing;

/// <inheritdoc />
internal sealed partial class RouteTable
{
    private static bool TryGetHeaderValues(HttpRequest request, string key, out string[] values)
    {
        if (request.Headers.TryGetValue(key, out var headerValues))
        {
            values = headerValues.ToArray()!;
            return true;
        }

        values = [];
        return false;
    }

    private static IReadOnlyDictionary<string, string[]> CaptureHeaders(IHeaderDictionary headers)
    {
        var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var header in headers)
        {
            result[header.Key] = header.Value.ToArray()!;
        }

        return result;
    }

    private static bool IsPreviewable(string? contentType) =>
        string.IsNullOrEmpty(contentType)
        || contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("text", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("javascript", StringComparison.OrdinalIgnoreCase)
        || contentType.Contains("urlencoded", StringComparison.OrdinalIgnoreCase);

    private static async Task<(byte[] Content, string? ContentType, int Length)> ReadRequestBodyAsync(HttpRequest request)
    {
        var contentType = request.ContentType;
        var declaredLength = request.ContentLength is { } length
            ? (int)Math.Min(length, int.MaxValue)
            : 0;

        if (request.ContentLength is 0 || request.Body is not { CanRead: true })
        {
            return ([], contentType, declaredLength);
        }

        request.EnableBuffering();
        const int limit = 8192;
        using var buffer = new MemoryStream();
        var chunk = new byte[Math.Min(4096, limit)];
        var remaining = limit;
        while (remaining > 0)
        {
            var read = await request.Body.ReadAsync(chunk.AsMemory(0, Math.Min(chunk.Length, remaining))).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            buffer.Write(chunk, 0, read);
            remaining -= read;
        }

        if (request.Body.CanSeek)
        {
            request.Body.Position = 0;
        }

        var content = buffer.ToArray();
        return (content, contentType, declaredLength > 0 ? declaredLength : content.Length);
    }

    private static JsonElement GenerateRoute(HttpRequest request, int status, string? contentType)
    {
        var response = new Dictionary<string, object?>
        {
            ["RequestHeaders"] = new Dictionary<string, string[]>(),
            ["ContentHeaders"] = new Dictionary<string, string[]>(),
            ["StatusCode"] = status
        };
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            response["ContentType"] = contentType;
        }

        var payload = new Dictionary<string, object?>
        {
            ["Request"] = new Dictionary<string, object>
            {
                ["Methods"] = new[] { new Dictionary<string, string> { ["Method"] = request.Method } },
                ["AbsoluteUri"] = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}"
            },
            ["Responses"] = new[] { response }
        };

        if (request.Headers.Authorization is { Count: > 0 } authorization)
        {
            payload["Authorization"] = new Dictionary<string, string[]>
            {
                ["Authorization"] = authorization.ToArray()!
            };
        }

        return JsonSerializer.SerializeToElement(payload);
    }

    private static string? BodyPreview(byte[]? content, string? contentType)
    {
        if (content is not { Length: > 0 } || !IsPreviewable(contentType))
        {
            return null;
        }

        var length = Math.Min(content.Length, 2048);
        var text = Encoding.UTF8.GetString(content.AsSpan(0, length));
        return content.Length > 2048 ? text + "…" : text;
    }
}
