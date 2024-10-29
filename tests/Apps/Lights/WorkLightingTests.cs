using NetDaemon.Apps.Lighting;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Lights;

public class WorkLightingTests : TestBase
{
    [TestCaseSource(nameof(WorkLightingLunchTestCases))]
    public void WorkLighting_VariousLunchStates_ExpectedDiningRoomLightState(string diningRoomLightsState, 
        string guestModeState, string personState, string workdaySensorState, DateTime date, bool expectedState)
    {
        HaMock.TriggerStateChange(Entities.Switch.DiningRoomLights, diningRoomLightsState);
        HaMock.TriggerStateChange(Entities.InputBoolean.ModeGuest, guestModeState);
        HaMock.TriggerStateChange(Entities.Person.Owen, personState);
        HaMock.TriggerStateChange(Entities.BinarySensor.WorkdaySensor, workdaySensorState);
        TestScheduler.AdvanceTo(date);
        HaMock.TriggerStateChange(Entities.Switch.OfficeLights, "on");

        Context.GetApp<WorkLighting>();
        HaMock.TriggerStateChange(Entities.Switch.OfficeLights, "off");
        
        Assert.That(Entities.Switch.DiningRoomLights.IsOn(), Is.EqualTo(expectedState));
    }
    
    private static IEnumerable<object> WorkLightingLunchTestCases()
    {
        yield return new object[] { "off", "off", "home", "on", new DateTime(2024, 01, 01, 12, 0, 0), true };
        yield return new object[] { "on", "off", "home", "on", new DateTime(2024, 01, 01, 12, 0, 0), true };
        yield return new object[] { "off", "on", "home", "on", new DateTime(2024, 01, 01, 12, 0, 0), false };
        yield return new object[] { "off", "off", "away", "on", new DateTime(2024, 01, 01, 12, 0, 0), false };
        yield return new object[] { "off", "off", "home", "off", new DateTime(2024, 01, 01, 12, 0, 0), false };
        yield return new object[] { "off", "off", "home", "on", new DateTime(2024, 01, 01, 18, 0, 0), false };
    }
    
    [TestCaseSource(nameof(WorkLightingMorningTestCases))]
    public void WorkLighting_VariousMorningStates_ExpectedDownstairsLightState(string downstairsLightState, 
        string guestModeState, string personState, string workdaySensorState, DateTime date, bool expectedState)
    {
        HaMock.TriggerStateChange(Entities.Light.DownstairsLights, downstairsLightState);
        HaMock.TriggerStateChange(Entities.InputBoolean.ModeGuest, guestModeState);
        HaMock.TriggerStateChange(Entities.Person.Owen, personState);
        HaMock.TriggerStateChange(Entities.BinarySensor.WorkdaySensor, workdaySensorState);
        TestScheduler.AdvanceTo(date);
        HaMock.TriggerStateChange(Entities.BinarySensor.UpstairsTvOn, "on");

        Context.GetApp<WorkLighting>();
        HaMock.TriggerStateChange(Entities.BinarySensor.UpstairsTvOn, "off");
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
        
        Assert.That(Entities.Light.DownstairsLights.IsOn(), Is.EqualTo(expectedState));
    }
    
    private static IEnumerable<object> WorkLightingMorningTestCases()
    {
        yield return new object[] { "off", "off", "home", "on", new DateTime(2024, 01, 01, 7, 0, 0), true };
        yield return new object[] { "on", "off", "home", "on", new DateTime(2024, 01, 01, 7, 0, 0), true };
        yield return new object[] { "off", "on", "home", "on", new DateTime(2024, 01, 01, 7, 0, 0), false };
        yield return new object[] { "off", "off", "away", "on", new DateTime(2024, 01, 01, 7, 0, 0), false };
        yield return new object[] { "off", "off", "home", "off", new DateTime(2024, 01, 01, 7, 0, 0), false };
        yield return new object[] { "off", "off", "home", "on", new DateTime(2024, 01, 01, 17, 0, 0), false };
    }
}