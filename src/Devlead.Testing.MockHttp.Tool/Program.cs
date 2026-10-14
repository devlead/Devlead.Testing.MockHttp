using Devlead.Console.Extensions;
using Devlead.Testing.MockHttp.Tool.Commands;

/// <summary>
/// Command-line host for the local mock HTTP API.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Registers the default <c>serve</c> command.
    /// </summary>
    /// <param name="appServiceConfig">The Devlead.Console application configuration.</param>
    static partial void ConfigureApp(AppServiceConfig appServiceConfig)
    {
        appServiceConfig
            .AddCommand<ServeCommand>("serve")
            .WithDescription("Host Routes.json as a local mock HTTP API.")
            .WithExample(["serve", "Resources", "--port", "5080"]);

        appServiceConfig.SetDefaultCommand<ServeCommand>();
        appServiceConfig.SetApplicationName("dotnet-mockhttp");
    }
}
