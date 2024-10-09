using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Tests.TestHelpers;

/// <summary>
/// Sets up context/mocks for tests to utilize.
/// </summary>
public class TestBase
{
    /// <summary>
    /// Sets up a new <see cref="TestContext"/> for the test to utilize.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        Context = new TestContext();
    }
    
    /// <summary>
    /// Context for the test to utilize.
    /// </summary>
    protected TestContext Context;
    
    /// <summary>
    /// Entities from the context.
    /// </summary>
    protected Entities Entities => Context.GetRequiredService<Entities>();
    
    /// <summary>
    /// Mock of the HA context from the test context.
    /// </summary>
    protected HaContextMock HaMock => Context.GetRequiredService<HaContextMock>();
}