using Devlead.Testing.MockHttp;
using Devlead.Testing.MockHttp.Tests;
using Devlead.Testing.MockHttp.Tests.Model;
using Devlead.Testing.MockHttp.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
public static partial class ServiceProviderFixture
{
    static partial void InitServiceProvider(IServiceCollection services)
    {
        services.AddSingleton<MyService>()
                .AddMockHttpClient<Constants>();

        services
          .AddOptions<MyServiceSettings>()
          .BindConfiguration(
              nameof(MyService)
          );
    }

    static partial void ConfigureInMemory(IDictionary<string, string?> configData)
    {
        configData.Add($"{nameof(MyService)}:ServiceName", nameof(MyService));
    }
}
