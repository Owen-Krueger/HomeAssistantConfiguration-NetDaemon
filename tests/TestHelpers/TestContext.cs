using System.Reactive.Concurrency;
using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Reactive.Testing;
using NetDaemon.HassModel;

namespace NetDaemon.Tests.TestHelpers;

public class TestContext : IServiceProvider
{
    private readonly IServiceCollection serviceCollection = new ServiceCollection();
    private readonly IServiceProvider serviceProvider;

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
    
    public object? GetService(Type serviceType) => serviceProvider.GetService(serviceType);

    public T GetApp<T>() => ActivatorUtilities.GetServiceOrCreateInstance<T>(serviceProvider);
    
    public Entities Entities => this.GetRequiredService<Entities>();
    
    public HaContextMock HaMock => this.GetRequiredService<HaContextMock>();
}