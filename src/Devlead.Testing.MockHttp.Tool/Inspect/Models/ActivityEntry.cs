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
    IReadOnlyDictionary<string, string[]> ResponseHeaders,
    string? ContentType,
    int ContentLength,
    string? BodyPreview);
