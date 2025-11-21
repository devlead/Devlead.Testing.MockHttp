using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Devlead.Testing.MockHttp;

public class Routes<T>
{
    public static Func<HttpRequestMessage, HttpResponseMessage> GetResponseBuilder(IServiceProvider provider)
    {
        var routes = GetRoutes();
        var shouldReplaceResponses = provider
                                    .GetServices<IServiceCollectionExtensions.ShouldReplaceResponse<T>>()
                                    .ToArray();

        HttpResponseMessage GetResponseBuilder(HttpRequestMessage request)
        {
            foreach(var shouldReplaceResponse in shouldReplaceResponses)
            {
                if (shouldReplaceResponse(provider, out var replacementResponse))
                {
                    return replacementResponse;
                }
            }

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
                        > GetRoutes()
     => InitializeRoutesFromResource();


    private static readonly
#if NET9_0_OR_GREATER
        Lock 
#else
        object
#endif
        _lock = new ();

    private static ImmutableDictionary<(HttpMethod Method, string PathAndQuery), Func<HttpRequestMessage, HttpResponseMessage>> InitializeRoutesFromResource()
    {
        static ImmutableArray<Route> GetRoutes()
        {
            lock (_lock)
            {
                return ImmutableArray.Create(System.Text.Json.JsonSerializer.Deserialize<Route[]>(Resources<T>.GetString("Routes.json") ?? throw new ArgumentNullException("routesJson")) ?? throw new ArgumentNullException("routes"));
            }
        }

        var routes = GetRoutes();

        var result =
            routes
            .Aggregate(
                (
                     EnableRoute: routes
                                    .Aggregate(
                                        new ConcurrentDictionary<(
                                            HttpMethod Method,
                                            string AbsoluteUri
                                            ),
                                            Action<bool>
                                            >(),
                                        static (seed, value) =>
                                        {
                                            void Enable(bool enabled) => value.Request.Disabled = !enabled;
                                            foreach (var method in value.Request.Methods)
                                            {
                                                seed.TryAdd((method, value.Request.AbsoluteUri), Enable);
                                            }

                                            return seed;
                                        },
                                        seed => seed.ToImmutableDictionary()
                                        ),
                    Routes: new ConcurrentDictionary<(
                                    HttpMethod Method,
                                    string AbsoluteUri
                                    ),
                                    Func<HttpRequestMessage, HttpResponseMessage>
                                    >()
                ),
                static (seed, value) =>
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

                                var httpResponse = AddHeaders(
                                    new HttpResponseMessage()
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


                                if (response.EnableRequests.Length != 0)
                                {
                                    foreach (var enableRequest in response.EnableRequests)
                                    {
                                        if (seed.EnableRoute.TryGetValue((enableRequest.Method, enableRequest.AbsoluteUri), out var enable))
                                        {
                                            enable(true);
                                        }
                                    }
                                }

                                if (response.DisableRequests.Length != 0)
                                {
                                    foreach (var disableRequest in response.DisableRequests)
                                    {
                                        if (seed.EnableRoute.TryGetValue((disableRequest.Method, disableRequest.AbsoluteUri), out var enable))
                                        {
                                            enable(false);
                                        }
                                    }
                                }

                                return httpResponse;
                            }

                            return new HttpResponseMessage
                            {
                                StatusCode = HttpStatusCode.BadRequest
                            };
                        }
                    );

                    foreach (var method in value.Request.Methods)
                    {
                        seed.Routes.TryAdd((method, value.Request.AbsoluteUri), responseFunc);
                    }

                    return seed;
                },
                static seed => seed.Routes.ToImmutableDictionary()
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
        
        string? ContentResource,
        string ContentType,
        HttpStatusCode StatusCode
        )
    {
        public Dictionary<string, string[]> RequestHeaders { get; init; } = [];
        public Dictionary<string, string[]> ContentHeaders { get; init; } = [];
        public RouteEnableRequest[] EnableRequests { get; init; } = [];
        public RouteEnableRequest[] DisableRequests { get; init; } = [];
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