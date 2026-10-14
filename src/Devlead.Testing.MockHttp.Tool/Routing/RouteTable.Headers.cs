using System.Text;

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

    private static string? BodyPreview(byte[]? content, string? contentType)
    {
        if (content is not { Length: > 0 } || contentType is null)
        {
            return null;
        }

        if (!contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
            && !contentType.Contains("text", StringComparison.OrdinalIgnoreCase)
            && !contentType.Contains("xml", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var length = Math.Min(content.Length, 2048);
        var text = Encoding.UTF8.GetString(content.AsSpan(0, length));
        return content.Length > 2048 ? text + "…" : text;
    }
}
