using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Moq;
using NetDaemon.Apps.Internet;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class InternetTests : TestBase
{
    [Test]
    public void Internet_InternetUpAfterRestart_OwenNotNotified()
    {
        var testScheduler = Context.GetRequiredService<TestScheduler>();
        testScheduler.AdvanceTo(new DateTime(2024, 01, 1).Ticks);
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "on");
        HaMock.TriggerStateChange(Entities.Switch.InternetModemSmartPlug, "on");
        
        Context.GetApp<Internet>();
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "off");
        testScheduler.AdvanceBy(TimeSpan.FromSeconds(91).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.False);
        testScheduler.AdvanceBy(TimeSpan.FromSeconds(16).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.True);
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "on");
        testScheduler.AdvanceBy(TimeSpan.FromMinutes(3).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Internet still down"))), Times.Never);
    }
    
    [Test]
    public void Internet_InternetDownAfterRestart_OwenNotified()
    {
        var testScheduler = Context.GetRequiredService<TestScheduler>();
        testScheduler.AdvanceTo(new DateTime(2024, 01, 1).Ticks);
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "on");
        HaMock.TriggerStateChange(Entities.Switch.InternetModemSmartPlug, "on");
        
        Context.GetApp<Internet>();
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "off");
        testScheduler.AdvanceBy(TimeSpan.FromSeconds(91).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.False);
        testScheduler.AdvanceBy(TimeSpan.FromSeconds(16).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.True);
        testScheduler.AdvanceBy(TimeSpan.FromMinutes(3).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Internet still down"))), Times.Once);
    }
}