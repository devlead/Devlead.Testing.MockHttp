using System.Text.Json;

namespace Devlead.Testing.MockHttp.Tool.Inspect.Models;

internal sealed record ActivityEntry(
    string Id,
    DateTimeOffset Utc,
    string Method,
    string PathAndQuery,
    string Host,
    int Status,
    string MatchedAbsoluteUri,
    bool EnableDisable,
    IReadOnlyDictionary<string, string[]> RequestHeaders,
    string? RequestContentType,
    int RequestContentLength,
    string? RequestBodyPreview,
    IReadOnlyDictionary<string, string[]> ResponseHeaders,
    string? ContentType,
    int ContentLength,
    string? BodyPreview,
    JsonElement Route);
