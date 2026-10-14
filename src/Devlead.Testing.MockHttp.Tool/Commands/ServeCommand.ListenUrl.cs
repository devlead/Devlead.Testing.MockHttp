using System.Net;

namespace Devlead.Testing.MockHttp.Tool.Commands;

/// <inheritdoc />
public sealed partial class ServeCommand
{
    /// <summary>
    /// Resolves the Kestrel listen URL from <see cref="ServeSettings.Url"/> or <see cref="ServeSettings.Port"/>.
    /// </summary>
    /// <param name="settings">The command settings.</param>
    /// <returns>The listen authority, for example <c>http://127.0.0.1:5080</c>.</returns>
    internal static string ResolveListenUrl(ServeSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Url))
        {
            if (!Uri.TryCreate(settings.Url, UriKind.Absolute, out var uri))
            {
                throw new ArgumentException("The --url value must be an absolute URI.", nameof(settings));
            }

            if (!IsLoopbackHost(uri.Host))
            {
                AnsiConsole.MarkupLine(
                    "[yellow]Warning:[/] listening on a non-loopback address. The mock API has no authentication beyond Routes.json.");
            }

            return uri.GetLeftPart(UriPartial.Authority);
        }

        var port = settings.Port is > 0 ? settings.Port.Value : 5080;
        return $"http://127.0.0.1:{port}";
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="host"/> is loopback or <c>localhost</c>.
    /// </summary>
    /// <param name="host">The URI host.</param>
    /// <returns><see langword="true"/> when the host is loopback.</returns>
    private static bool IsLoopbackHost(string host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
        (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));
}
