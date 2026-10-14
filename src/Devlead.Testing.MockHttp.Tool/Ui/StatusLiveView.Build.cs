using Spectre.Console.Rendering;

namespace Devlead.Testing.MockHttp.Tool.Ui;

/// <inheritdoc />
internal static partial class StatusLiveView
{
    internal static IRenderable Build(RouteTable table, string listenUrl, string inspectUrl)
    {
        var snapshot = table.Snapshot(listenUrl, string.Empty);
        var counts = snapshot.Counts;

        var header = new Markup(
            $"[bold]Listening[/] [green]{Markup.Escape(listenUrl)}[/]   [bold]Inspect[/] [link]{Markup.Escape(inspectUrl)}[/]\n" +
            $"[grey]{Markup.Escape(snapshot.ResourcesDirectory)}[/]   match-host: {(snapshot.MatchHost ? "[green]on[/]" : "[grey]off[/]")}\n" +
            $"Routes {counts.Routes}  Enabled {counts.Enabled}  Requests {counts.Requests}  401 {counts.Unauthorized}  404 {counts.NotFound}");

        var routes = new Table().Title("Routes").Expand();
        routes.AddColumn("Method");
        routes.AddColumn("Path");
        routes.AddColumn("Host");
        routes.AddColumn("Enabled");
        routes.AddColumn("Hits");
        foreach (var route in snapshot.Routes.Take(20))
        {
            routes.AddRow(
                Markup.Escape(route.Method),
                Markup.Escape(route.PathAndQuery),
                Markup.Escape(route.Host),
                route.Enabled ? "[green]yes[/]" : "[red]no[/]",
                route.Hits.ToString());
        }

        var activity = new Table().Title("Activity").Expand();
        activity.AddColumn("UTC");
        activity.AddColumn("Method");
        activity.AddColumn("Path");
        activity.AddColumn("Status");
        activity.AddColumn("Matched");
        foreach (var entry in snapshot.Activity.Take(15))
        {
            activity.AddRow(
                Markup.Escape(entry.Utc.ToString("HH:mm:ss")),
                Markup.Escape(entry.Method),
                Markup.Escape(entry.PathAndQuery),
                entry.Status.ToString(),
                Markup.Escape(entry.MatchedAbsoluteUri));
        }

        return new Rows(header, routes, activity);
    }
}
