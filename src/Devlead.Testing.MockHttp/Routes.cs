using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json.Serialization;

namespace Devlead.Testing.MockHttp;

public class Routes<T>
{
    public static Func<HttpRequestMessage, HttpResponseMessage> GetResponseBuilder()
    {
        var routes = GetRoutes();

        HttpResponseMessage GetResponseBuilder(HttpRequestMessage request)
        {
            if (
                   request.RequestUri?.AbsoluteUri is { } absoluteUri
                   &&
                   routes.TryGetValue(
                   (
                       request.Method,
                       absoluteUri
                   ),
                   out var response
                   )
               )
            {
                return response(request);
            }

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };
        }

        return GetResponseBuilder;
    }

    private static ImmutableDictionary<(
                        HttpMethod Method,
                        string AbsoluteUri
                        ),
                        Func<HttpRequestMessage, HttpResponseMessage>
                        >? _routes;

    private static ImmutableDictionary<(
                        HttpMethod Method,
                        string AbsoluteUri
                        ),
                        Func<HttpRequestMessage, HttpResponseMessage>
                        > GetRoutes()
     => _routes ??= InitializeRoutesFromResource();


    private static ImmutableDictionary<(HttpMethod Method, string PathAndQuery), Func<HttpRequestMessage, HttpResponseMessage>> InitializeRoutesFromResource()
    {
        var routesJson = Resources<T>.GetString("Routes.json");
        ArgumentException.ThrowIfNullOrEmpty(routesJson);

        var routes = System.Text.Json.JsonSerializer.Deserialize<Route[]>(routesJson);
        ArgumentNullException.ThrowIfNull(routes);

        var enableRoute = routes
                .Aggregate(
                    new Dictionary<(
                        HttpMethod Method,
                        string AbsoluteUri
                        ),
                        Action
                        >(),
                    (seed, value) =>
                    {
                        void Enable() => value.Request.Disabled = false;
                        foreach (var method in value.Request.Methods)
                        {
                            seed[(method, value.Request.AbsoluteUri)] = Enable;
                        }

                        return seed;
                    },
                    seed => seed.ToImmutableDictionary()
                    );

        var result =
            routes
            .Aggregate(
                new Dictionary<(
                    HttpMethod Method,
                    string AbsoluteUri
                    ),
                    Func<HttpRequestMessage, HttpResponseMessage>
                    >(),
                (seed, value) =>
                {
                    static HttpResponseMessage AddHeaders(HttpResponseMessage response, Dictionary<string, string[]> headers)
                    {
                        foreach (var (key, value) in headers)
                        {
                            response.Headers.TryAddWithoutValidation(key, value);
                        }

                        return response;
                    }

                    var responseFunc = new Func<HttpRequestMessage, HttpResponseMessage>(
                        request =>
                        {
                            if (value.Request.Disabled)
                            {
                                return new HttpResponseMessage
                                {
                                    StatusCode = HttpStatusCode.NotFound
                                };
                            }

                            if (value.Authorization is { } authorization)
                            {
                                foreach (var header in authorization)
                                {
                                    if (!request.Headers.TryGetValues(header.Key, out var values) || !values.All(value => header.Value.Contains(value)))
                                    {
                                        return new HttpResponseMessage
                                        {
                                            StatusCode = HttpStatusCode.Unauthorized
                                        };
                                    }
                                }
                            }

                            var result = value.Responses.FirstOrDefault(
                                response => response.RequestHeaders.All(
                                    header =>
                                    request.Headers.TryGetValues(header.Key, out var values) && values.All(value => header.Value.Contains(value))
                                    ||
                                    request.Content?.Headers.TryGetValues(header.Key, out var contentValues) == true && contentValues.All(value => header.Value.Contains(value))
                                    )
                            );

                            if (result is { } response)
                            {
                                if (response.EnableRequests.Any())
                                {
                                    foreach (var enableRequest in response.EnableRequests)
                                    {
                                        if (enableRoute.TryGetValue((enableRequest.Method, enableRequest.AbsoluteUri), out var enable))
                                        {
                                            enable();
                                        }
                                    }
                                }

                                return AddHeaders(new HttpResponseMessage()
                                {
                                    Content = !string.IsNullOrWhiteSpace(response.ContentResource) && Resources<T>.GetBytes(response.ContentResource) is { } content
                                                ? new ByteArrayContent(
                                                    content
                                                )
                                                {
                                                    Headers =
                                                    {
                                                        ContentType = MediaTypeHeaderValue.Parse(response.ContentType),
                                                        ContentMD5 = System.Security.Cryptography.MD5.HashData(content)
                                                    }
                                                }
                                                : null,
                                    StatusCode = response.StatusCode
                                },
                                response.ContentHeaders
                                );
                            }

                            return new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.BadRequest
                            };
                        }
                    );

                    foreach (var method in value.Request.Methods)
                    {
                        seed[(method, value.Request.AbsoluteUri)] = responseFunc;
                    }

                    return seed;
                },
                seed => seed.ToImmutableDictionary()
                );

        return result;
    }

    public record Route(
        RouteRequest Request,
        RouteResponse[] Responses,
        Dictionary<string, string[]>? Authorization = null
        );

    public record RouteRequest(
        HttpMethod[] Methods,
        string AbsoluteUri
        )
    {
        public bool Disabled { get; set; }
    }

    public record RouteResponse(
        Dictionary<string, string[]> RequestHeaders,
        string? ContentResource,
        string ContentType,
        Dictionary<string, string[]> ContentHeaders,
        HttpStatusCode StatusCode
        )
    {
        public RouteEnableRequest[] EnableRequests { get; init; } = [];
    }

    public record RouteEnableRequest(
        [property:JsonPropertyName("Method")]
        string StringMethod,
        string AbsoluteUri
        )
    {
        internal HttpMethod Method { get; init; } = new(StringMethod);
    }
}