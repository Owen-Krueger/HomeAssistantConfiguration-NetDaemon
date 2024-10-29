using NetDaemon.Apps.Lighting;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Lights;

public class OutsideLightingTests : TestBase
{
    [Test]
    public void OutsideLighting_LatePresenceAutomations_LightsTurnedOn()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 1, 1, 0, 0, 0));
        HaMock.TriggerStateChange(Entities.Switch.FrontPorchLights, "off");
        HaMock.TriggerStateChange(Entities.InputBoolean.HolidayMode, "on");
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Sensor.AllisonDistanceMiles, "6");
        HaMock.TriggerStateChange(Entities.Sensor.SunNextRising, "2024-01-02T07:00:00.0000000Z");

        Context.GetApp<OutsideLighting>();
        HaMock.TriggerStateChange(Entities.Sensor.AllisonDistanceMiles, "5");
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Switch.FrontPorchLights.IsOn(), Is.True);
            Assert.That(Entities.Group.HolidayLights.IsOn(), Is.False);
        });
        
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Switch.FrontPorchLights.IsOn(), Is.False);
            Assert.That(Entities.Group.HolidayLights.IsOn(), Is.False);
        });
    }

    [Test]
    public void OutsideLighting_PersonDetected_LightsTurnedOn()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 1, 1, 0, 0, 0));
        HaMock.TriggerStateChange(Entities.Switch.FrontPorchLights, "off");
        HaMock.TriggerStateChange(Entities.InputBoolean.HolidayMode, "on");
        HaMock.TriggerStateChange(Entities.BinarySensor.G4DoorbellProPersonDetected, "off");
        HaMock.TriggerStateChange(Entities.Sensor.SunNextRising, "2024-01-02T07:00:00.0000000Z");
        
        Context.GetApp<OutsideLighting>();
        HaMock.TriggerStateChange(Entities.BinarySensor.G4DoorbellProPersonDetected, "on");
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Switch.FrontPorchLights.IsOn(), Is.True);
            Assert.That(Entities.Group.HolidayLights.IsOn(), Is.False);
        });
        
        HaMock.TriggerStateChange(Entities.BinarySensor.G4DoorbellProPersonDetected, "off");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(2).Ticks);
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Switch.FrontPorchLights.IsOn(), Is.False);
            Assert.That(Entities.Group.HolidayLights.IsOn(), Is.False);
        });
    }
}