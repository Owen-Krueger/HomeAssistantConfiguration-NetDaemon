﻿using HomeAssistantGenerated;
using Moq;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Internet;

public class InternetTests : TestBase
{
    [Test]
    public void Internet_InternetUpAfterRestart_OwenNotNotified()
    {
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "on");
        HaMock.TriggerStateChange(Entities.Switch.InternetModemSmartPlug, "on");
        
        Context.GetApp<NetDaemon.Apps.Internet.Internet>();
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "off");
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(90).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.False);
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(15).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.True);
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "on");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(3).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Internet still down"))), Times.Never);
    }
    
    [Test]
    public void Internet_InternetDownAfterRestart_OwenNotified()
    {
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "on");
        HaMock.TriggerStateChange(Entities.Switch.InternetModemSmartPlug, "on");
        
        Context.GetApp<NetDaemon.Apps.Internet.Internet>();
        HaMock.TriggerStateChange(Entities.BinarySensor.InternetUp, "off");
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(90).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.False);
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(15).Ticks);
        
        Assert.That(Entities.Switch.InternetModemSmartPlug.IsOn(), Is.True);
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(3).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Internet still down"))), Times.Once);
    }
}