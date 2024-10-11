using Moq;
using NetDaemon.Apps.Walk;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class WalkTests : TestBase
{
    [Test]
    public void Walk_VariousWalkingSituations_BooleanUpdated()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 01, 01, 8, 0, 0));
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");

        Context.GetApp<Walk>();
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.Person.Owen, "home");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        HaMock.VerifyServiceCalled(Entities.InputBoolean.OwenOnMorningWalk, "input_boolean", "turn_off", null, Times.Once());
    }

    [Test]
    public void Walk_NotMorning_BooleanNotUpdated()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 01, 01, 11, 0, 0));
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "locked");
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "open");

        Context.GetApp<Walk>();
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "unlocked");
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");
        HaMock.VerifyServiceCalled(Entities.InputBoolean.OwenOnMorningWalk, "input_boolean", "turn_off", null, Times.Never());
    }
    
    [Test]
    public void Walk_FrontDoorOpened_BooleanTurnedOff()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 01, 01, 8, 0, 0));
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.Person.Owen, "home");
        HaMock.TriggerStateChange(Entities.Sensor.OwenDistanceMiles, "0");
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "locked");

        Context.GetApp<Walk>();
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "unlocked");
        HaMock.VerifyServiceCalled(Entities.InputBoolean.OwenOnMorningWalk, "input_boolean", "turn_off", null, Times.Once());
    }
    
    [Test]
    public void Walk_GarageDoorOpened_BooleanTurnedOff()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 01, 01, 8, 0, 0));
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.Person.Owen, "home");
        HaMock.TriggerStateChange(Entities.Sensor.OwenDistanceMiles, "0");
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "open");

        Context.GetApp<Walk>();
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");
        HaMock.VerifyServiceCalled(Entities.InputBoolean.OwenOnMorningWalk, "input_boolean", "turn_off", null, Times.Once());
    }

    [Test]
    public void Walk_BooleanAlreadyCorrect_BooleanNotUpdated()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 01, 01, 8, 0, 0));
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.Person.Owen, "home");
        HaMock.TriggerStateChange(Entities.BinarySensor.WorkdaySensor, "on");
        HaMock.TriggerStateChange(Entities.Sensor.OwenPhoneNetworkType, "cellular");
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "locked");
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "open");

        Context.GetApp<Walk>();
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        HaMock.VerifyServiceCalled(Entities.InputBoolean.OwenOnMorningWalk, "input_boolean", "turn_on", null, Times.Never());
        
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "off");
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "unlocked");
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");
        HaMock.VerifyServiceCalled(Entities.InputBoolean.OwenOnMorningWalk, "input_boolean", "turn_off", null, Times.Never());
    }
}