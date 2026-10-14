using System.ComponentModel;
using Spectre.Console.Cli;

namespace Devlead.Testing.MockHttp.Tool.Commands;

/// <summary>
/// Command-line settings for the local mock HTTP host.
/// </summary>
public sealed class ServeSettings : CommandSettings
{
    /// <summary>
    /// Gets the directory that contains <c>Routes.json</c> and response files.
    /// </summary>
    [CommandArgument(0, "<RESOURCES>")]
    [Description("Directory that contains Routes.json and response files.")]
    public string Resources { get; init; } = string.Empty;

    /// <summary>
    /// Gets the TCP port to listen on when <see cref="Url"/> is not set.
    /// </summary>
    [CommandOption("--port")]
    [Description("Port to listen on. Default is 5080. Ignored when --url is set.")]
    public int? Port { get; init; }

    /// <summary>
    /// Gets an absolute listen URL. When set, it overrides <see cref="Port"/>.
    /// </summary>
    [CommandOption("--url")]
    [Description("Absolute listen URL, for example http://127.0.0.1:5080. Overrides --port.")]
    public string? Url { get; init; }

    /// <summary>
    /// Gets a value indicating whether inbound requests must match the host from <c>AbsoluteUri</c>.
    /// </summary>
    [CommandOption("--match-host")]
    [Description("Require the request Host to match the host in Routes.json AbsoluteUri. Default is path and query only.")]
    public bool MatchHost { get; init; }
}
