using System.Reactive.Concurrency;
using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using NetDaemon.HassModel;

namespace NetDaemon.Tests.TestHelpers;

/// <summary>
/// Context for tests to utilize.
/// </summary>
public class TestContext : IServiceProvider
{
    private readonly IServiceCollection serviceCollection = new ServiceCollection();
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Sets up the context, with all the services an app/test may require.
    /// </summary>
    public TestContext()
    {
        serviceCollection.AddHomeAssistantGenerated();
        serviceCollection.AddSingleton(_ => new HaContextMock());
        serviceCollection.AddTransient<IHaContext>(x => x.GetRequiredService<HaContextMock>().Object);
        serviceCollection.AddTransient<HaContextMockImpl>(x => x.GetRequiredService<HaContextMock>().Object);
        serviceCollection.AddSingleton<TestScheduler>();
        serviceCollection.AddTransient<IScheduler>(x => x.GetRequiredService<TestScheduler>());
        serviceCollection.AddTransient(typeof(ILogger<>), typeof(NullLogger<>));
        
        serviceProvider = serviceCollection.BuildServiceProvider();
    }
    
    /// <summary>
    /// Gets the <see cref="serviceType"/> from the <see cref="serviceProvider"/>.
    /// </summary>
    public object? GetService(Type serviceType) => serviceProvider.GetService(serviceType);

    /// <summary>
    /// Gets a NetDaemon app from the <see cref="serviceProvider"/>.
    /// </summary>
    public T GetApp<T>() => ActivatorUtilities.GetServiceOrCreateInstance<T>(serviceProvider);
    
    /// <summary>
    /// Entities from the <see cref="serviceProvider"/>.
    /// </summary>
    public Entities Entities => this.GetRequiredService<Entities>();
    
    /// <summary>
    /// Mock of the HA context from the <see cref="serviceProvider"/>.
    /// </summary>
    public HaContextMock HaMock => this.GetRequiredService<HaContextMock>();
}