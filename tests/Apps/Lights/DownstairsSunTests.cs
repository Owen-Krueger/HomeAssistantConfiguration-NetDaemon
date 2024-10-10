using HomeAssistantGenerated;
using NetDaemon.Apps.Lighting;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Lights;

public class DownstairsSunTests : TestBase
{
    [Test]
    public void DownstairsLights_DownstairsLightsTurnedOn_BrightnessSet()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.InputBoolean.LightAutomaticallyDimDownstairsLights, "on");
        HaMock.TriggerStateChange(Entities.Light.DownstairsLights, "off");
        HaMock.TriggerStateChange(Entities.Sun.Sun, "", new SunAttributes { Elevation = 5 });
        
        Context.GetApp<DownstairsSun>();
        HaMock.TriggerStateChange(Entities.Light.DownstairsLights, "on", new LightTurnOnParameters { Brightness = 255 });
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Light.DownstairsLights.IsOn(), Is.True);
            Assert.That(Entities.Light.DownstairsLights.Attributes!.Brightness!.AsJsonElement().GetInt32(), Is.EqualTo(128));
        });
        
        HaMock.TriggerStateChange(Entities.Light.DownstairsLights, "off");
        HaMock.TriggerStateChange(Entities.Sun.Sun, "", new SunAttributes { Elevation = 15 });
        HaMock.TriggerStateChange(Entities.Light.DownstairsLights, "on", new LightTurnOnParameters { Brightness = 128 });
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Light.DownstairsLights.IsOn(), Is.True);
            Assert.That(Entities.Light.DownstairsLights.Attributes!.Brightness!.AsJsonElement().GetInt32(), Is.EqualTo(255));
        });
    }
    
    [Test]
    public void DownstairsLights_AutomationsTurnedOff_BrightnessNotSet()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.InputBoolean.LightAutomaticallyDimDownstairsLights, "off");
        HaMock.TriggerStateChange(Entities.Light.DownstairsLights, "off");
        HaMock.TriggerStateChange(Entities.Sun.Sun, "", new SunAttributes { Elevation = 5 });
        
        Context.GetApp<DownstairsSun>();
        HaMock.TriggerStateChange(Entities.Light.DownstairsLights, "on", new LightTurnOnParameters { Brightness = 255 });
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(5).Ticks);
        Assert.Multiple(() =>
        {
            Assert.That(Entities.Light.DownstairsLights.IsOn(), Is.True);
            Assert.That(Entities.Light.DownstairsLights.Attributes!.Brightness!.AsJsonElement().GetInt32(), Is.EqualTo(255));
        });
    }
    
    // [Test]
    // public void DownstairsLights_SunElevationChanged_BrightnessSet()
    // {
    //     TestScheduler.AdvanceToNow();
    //     HaMock.TriggerStateChange(Entities.InputBoolean.LightAutomaticallyDimDownstairsLights, "on");
    //     HaMock.TriggerStateChange(Entities.Light.DownstairsLights, "on", new LightTurnOnParameters { Brightness = 255});
    //     HaMock.TriggerStateChange(Entities.Sun.Sun, "", new SunAttributes { Elevation = 10 });
    //     
    //     Context.GetApp<DownstairsSun>();
    //     HaMock.TriggerStateChange(Entities.Sun.Sun, "", new SunAttributes { Elevation = 9 });
    //     Assert.That(Entities.Light.DownstairsLights.Attributes!.Brightness!.AsJsonElement().GetInt32(), Is.EqualTo(128));
    //     
    //     HaMock.TriggerStateChange(Entities.Sun.Sun, "", new SunAttributes { Elevation = 10 });
    //     Assert.That(Entities.Light.DownstairsLights.Attributes!.Brightness!.AsJsonElement().GetInt32(), Is.EqualTo(255));
    // }
}