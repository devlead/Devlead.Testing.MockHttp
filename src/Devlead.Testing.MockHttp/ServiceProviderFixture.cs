using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

public static partial class ServiceProviderFixture
{
    public static (T1, T2, T3, T4, T5, T6, T7) GetRequiredService<T1, T2, T3, T4, T5, T6, T7>(
      Func<IServiceCollection, IServiceCollection>? configure = null
      ) where T1 : notnull
           where T2 : notnull
           where T3 : notnull
           where T4 : notnull
           where T5 : notnull
           where T6 : notnull
           where T7 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>(),
            provider.GetRequiredService<T4>(),
            provider.GetRequiredService<T5>(),
            provider.GetRequiredService<T6>(),
            provider.GetRequiredService<T7>()
            );
    }
    public static (T1, T2, T3, T4, T5, T6) GetRequiredService<T1, T2, T3, T4, T5, T6>(
       Func<IServiceCollection, IServiceCollection>? configure = null
       ) where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull
            where T6 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>(),
            provider.GetRequiredService<T4>(),
            provider.GetRequiredService<T5>(),
            provider.GetRequiredService<T6>()
            );
    }

    public static (T1, T2, T3, T4, T5) GetRequiredService<T1, T2, T3, T4, T5>(
       Func<IServiceCollection, IServiceCollection>? configure = null
       ) where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
            where T5 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>(),
            provider.GetRequiredService<T4>(),
            provider.GetRequiredService<T5>()
            );
    }

    public static (T1, T2, T3, T4) GetRequiredService<T1, T2, T3, T4>(
       Func<IServiceCollection, IServiceCollection>? configure = null
       ) where T1 : notnull
            where T2 : notnull
            where T3 : notnull
            where T4 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>(),
            provider.GetRequiredService<T4>()
            );
    }

    public static (T1, T2, T3) GetRequiredService<T1, T2, T3>(
        Func<IServiceCollection, IServiceCollection>? configure = null
        ) where T1 : notnull
            where T2 : notnull
            where T3 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>(),
            provider.GetRequiredService<T3>()
            );
    }

    public static (T1, T2) GetRequiredService<T1, T2>(
        Func<IServiceCollection, IServiceCollection>? configure = null
        ) where T1 : notnull
            where T2 : notnull
    {
        var provider = GetServiceProvider(configure);
        return (
            provider.GetRequiredService<T1>(),
            provider.GetRequiredService<T2>()
            );
    }

    public static T GetRequiredService<T>(
        Func<IServiceCollection, IServiceCollection>? configure = null
        ) where T : notnull
        => GetServiceProvider(configure)
            .GetRequiredService<T>();

    public static ServiceProvider GetServiceProvider(Func<IServiceCollection, IServiceCollection>? configure)
    {
        var inMemoryConfigurationData = new Dictionary<string, string?>();
        ConfigureInMemory(inMemoryConfigurationData);


        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemoryConfigurationData);

        Configure(configurationBuilder);

        var configuration = configurationBuilder
                            .Build();

        var serviceCollection = new ServiceCollection();
        
        serviceCollection
                .AddSingleton((IConfiguration)configuration)
                .AddSingleton<FakeTimeProvider>()
                .AddSingleton<TimeProvider>(provider => provider.GetRequiredService<FakeTimeProvider>());

        InitServiceProvider(serviceCollection);
        return (configure?.Invoke(serviceCollection) ?? serviceCollection).BuildServiceProvider();
    }

    static partial void ConfigureInMemory(IDictionary<string, string?> configData);

    static partial void Configure(IConfigurationBuilder configuration);

    static partial void InitServiceProvider(IServiceCollection services);
}