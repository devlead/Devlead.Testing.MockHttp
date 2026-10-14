namespace Devlead.Testing.MockHttp.Tool.Ui;

/// <summary>
/// Renders a live Spectre.Console view of listen URL, routes, and recent traffic.
/// </summary>
internal static partial class StatusLiveView
{
    /// <summary>
    /// Renders the live status view until <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    public static async Task RunAsync(
        RouteTable table,
        string listenUrl,
        CancellationToken cancellationToken)
    {
        var inspectUrl = $"{listenUrl.TrimEnd('/')}/inspect";
        var view = Build(table, listenUrl, inspectUrl);
        if (System.Console.IsOutputRedirected || !Environment.UserInteractive)
        {
            AnsiConsole.Write(view);
            await WaitUntilCancelled(cancellationToken).ConfigureAwait(false);
            return;
        }

        try
        {
            await AnsiConsole.Live(view)
                .AutoClear(false)
                .StartAsync(async ctx =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        ctx.UpdateTarget(Build(table, listenUrl, inspectUrl));
                        await WaitUntilCancelled(cancellationToken, TimeSpan.FromMilliseconds(500)).ConfigureAwait(false);
                    }
                })
                .ConfigureAwait(false);
        }
        catch (IOException)
        {
            AnsiConsole.Write(Build(table, listenUrl, inspectUrl));
            await WaitUntilCancelled(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static async Task WaitUntilCancelled(CancellationToken cancellationToken, TimeSpan? delay = null)
    {
        try
        {
            await Task.Delay(delay ?? Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
