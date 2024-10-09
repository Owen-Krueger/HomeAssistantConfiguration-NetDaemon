using System.Text.Json;
using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Lighting;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class BedroomLightsTests : TestBase
{
    // 10 PM Central
    private readonly DateTimeOffset lateTime = new(2024, 01, 02, 4, 0, 0, TimeSpan.Zero);
    
    [Test]
    public void BedroomLighting_BedroomLampsTurnOn_BedroomLightsTurnedOff()
    {
        TestScheduler.AdvanceTo(lateTime.Ticks);
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "on");
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, "off");

        Context.GetApp<BedroomLighting>();
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, "on");
        Assert.That(Entities.Switch.BedroomLights.IsOn(), Is.False);
    }
    
    [Test]
    public void BedroomLighting_BedroomLightsTurnOff_BedroomLampsTurnedOn()
    {
        // 10 PM central
        TestScheduler.AdvanceTo(lateTime.Ticks);
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "on");
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, "off");

        Context.GetApp<BedroomLighting>();
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "off");
        Assert.That(Entities.Light.BedroomLamps.IsOn(), Is.True);
    }

    [Test]
    public void BedroomLighting_NotLate_NightLightingNotActivated()
    {
        TestScheduler.AdvanceTo(new DateTimeOffset(2024, 01, 02, 0, 0, 0, TimeSpan.Zero).Ticks);
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "on");
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, "off");

        Context.GetApp<BedroomLighting>();
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "off");
        Assert.That(Entities.Light.BedroomLamps.IsOn(), Is.False);

        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "on");
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, "on");
        Assert.That(Entities.Switch.BedroomLights.IsOn(), Is.True);
    }

    [Test]
    public void BedroomLighting_BedsideButtonPressed_LampsToggled()
    {
        TestScheduler.AdvanceTo(lateTime.Ticks);
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "on");
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, "on");
        Context.GetApp<BedroomLighting>();

        var zhaEvent = new Event
        {
            EventType = EventTypes.ZhaEvent,
            DataElement = JsonSerializer.SerializeToElement(new ZhaEvent
                { DeviceId = DeviceIds.BedsideButtonId, Command = "single" })
        };
        HaMock.TriggerEvent(zhaEvent);
        HaMock.Verify(x => x.CallService("light", "toggle",
            It.IsAny<ServiceTarget>(),
            It.IsAny<LightToggleParameters>()), Times.Once);
        // Normally, the lamp should start off and turn on to turn the lights off, but toggle doesn't actually
        // update state in unit tests.
        Assert.That(Entities.Switch.BedroomLights.IsOn, Is.False);
    }
    
    [Test]
    public void BedroomLighting_BedsideButtonPressedNotLate_LightsRemainOn()
    {
        TestScheduler.AdvanceTo(new DateTimeOffset(2024, 01, 02, 0, 0, 0, TimeSpan.Zero).Ticks);
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, "on");
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, "on");
        Context.GetApp<BedroomLighting>();

        var zhaEvent = new Event
        {
            EventType = EventTypes.ZhaEvent,
            DataElement = JsonSerializer.SerializeToElement(new ZhaEvent
                { DeviceId = DeviceIds.BedsideButtonId, Command = "single" })
        };
        HaMock.TriggerEvent(zhaEvent);
        HaMock.Verify(x => x.CallService("light", "toggle",
            It.IsAny<ServiceTarget>(),
            It.IsAny<LightToggleParameters>()), Times.Once);
        Assert.That(Entities.Switch.BedroomLights.IsOn, Is.True);
    }
    
    [TestCase("off", "on")]
    [TestCase("on", "off")]
    public void BedroomLighting_BedsideButtonPressed_LightsRemainOn(string lightsState, string lampsState)
    {
        TestScheduler.AdvanceTo(lateTime.Ticks);
        HaMock.TriggerStateChange(Entities.Switch.BedroomLights, lightsState);
        HaMock.TriggerStateChange(Entities.Light.BedroomLamps, lampsState);
        Context.GetApp<BedroomLighting>();

        var zhaEvent = new Event
        {
            EventType = EventTypes.ZhaEvent,
            DataElement = JsonSerializer.SerializeToElement(new ZhaEvent
                { DeviceId = DeviceIds.BedsideButtonId, Command = "single" })
        };
        HaMock.TriggerEvent(zhaEvent);
        HaMock.Verify(x => x.CallService("light", "toggle",
            It.IsAny<ServiceTarget>(),
            It.IsAny<LightToggleParameters>()), Times.Once);
        Assert.That(Entities.Switch.BedroomLights.State, Is.EqualTo(lightsState));
    }
}