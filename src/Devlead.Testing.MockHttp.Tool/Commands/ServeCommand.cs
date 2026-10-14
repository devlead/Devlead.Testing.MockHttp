using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Devlead.Testing.MockHttp.Tool.Commands;

/// <summary>
/// Loads <c>Routes.json</c> and hosts the mock HTTP API until the process is cancelled.
/// </summary>
public sealed partial class ServeCommand : AsyncCommand<ServeSettings>
{
    /// <inheritdoc />
    public override async Task<int> ExecuteAsync(CommandContext context, ServeSettings settings, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(settings.Resources))
        {
            AnsiConsole.MarkupLine("[red]A resources directory is required.[/]");
            return 1;
        }

        var resourcesDirectory = Path.GetFullPath(settings.Resources);
        if (!Directory.Exists(resourcesDirectory))
        {
            AnsiConsole.MarkupLine($"[red]Resources directory not found:[/] {resourcesDirectory}");
            return 1;
        }

        var listenUrl = ResolveListenUrl(settings);
        var table = RouteTable.Load(resourcesDirectory, settings.MatchHost);

        var builder = MockHttpWebHost.CreateBuilder(table, listenUrl);
        builder.Logging.ClearProviders();
        var app = builder.Build();
        MockHttpWebHost.Map(app);

        using var stop = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, app.Lifetime.ApplicationStopping);
        ConsoleCancelEventHandler? onCancel = (_, args) =>
        {
            args.Cancel = true;
            stop.Cancel();
        };
        System.Console.CancelKeyPress += onCancel;

        await app.StartAsync(stop.Token).ConfigureAwait(false);

        try
        {
            await StatusLiveView.RunAsync(table, listenUrl, stop.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stop.IsCancellationRequested)
        {
        }
        finally
        {
            System.Console.CancelKeyPress -= onCancel;
            await app.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }

        return 0;
    }
}
