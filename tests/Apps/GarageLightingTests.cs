using NetDaemon.Apps.Lighting;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class GarageLightingTests : TestBase
{
    [Test]
    public void GarageLighting_GarageDoorState_LightsTurnedOnAndOff()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");
        HaMock.TriggerStateChange(Entities.Switch.GarageLights, "off");
        HaMock.TriggerStateChange(Entities.BinarySensor.GarageLightsMotionDetection, "off");

        Context.GetApp<GarageLighting>();
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "opening");
        Assert.That(Entities.Switch.GarageLights.IsOn(), Is.True);
        
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        Assert.That(Entities.Switch.GarageLights.IsOn(), Is.False);
    }
    
    [Test]
    public void GarageLighting_MotionDetected_LightsTurnedOnAndOff()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");
        HaMock.TriggerStateChange(Entities.Switch.GarageLights, "off");
        HaMock.TriggerStateChange(Entities.BinarySensor.GarageLightsMotionDetection, "off");

        Context.GetApp<GarageLighting>();
        HaMock.TriggerStateChange(Entities.BinarySensor.GarageLightsMotionDetection, "on");
        Assert.That(Entities.Switch.GarageLights.IsOn(), Is.True);
        
        HaMock.TriggerStateChange(Entities.BinarySensor.GarageLightsMotionDetection, "off");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        Assert.That(Entities.Switch.GarageLights.IsOn(), Is.False);
    }
    
    [Test]
    public void GarageLighting_GarageLightsAlreadyOn_LightsUnchanged()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");
        HaMock.TriggerStateChange(Entities.Switch.GarageLights, "on");
        HaMock.TriggerStateChange(Entities.BinarySensor.GarageLightsMotionDetection, "off");

        Context.GetApp<GarageLighting>();
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "opening");
        Assert.That(Entities.Switch.GarageLights.IsOn(), Is.True);
    }
}