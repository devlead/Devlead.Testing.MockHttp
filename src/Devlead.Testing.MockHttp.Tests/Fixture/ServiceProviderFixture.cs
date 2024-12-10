using Devlead.Testing.MockHttp;
using Devlead.Testing.MockHttp.Tests;
using Devlead.Testing.MockHttp.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
public static partial class ServiceProviderFixture
{
    static partial void InitServiceProvider(IServiceCollection services)
    {
        services.AddSingleton<MyService>()
                .AddMockHttpClient<Constants>();
    }
}
