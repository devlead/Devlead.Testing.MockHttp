using System.Text.Json.Serialization;

namespace Devlead.Testing.MockHttp.Tool.Routing.Models;

internal sealed record RouteEnableRequest(
    [property: JsonPropertyName("Method")]
    string StringMethod,
    string AbsoluteUri
)
{
    internal HttpMethod Method { get; } = new(StringMethod);
}
