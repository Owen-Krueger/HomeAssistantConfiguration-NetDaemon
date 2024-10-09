using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Security;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Security;

public class FrontDoorSecurityTests : TestBase
{
    [Test]
    public void FrontDoorSecurity_NobodyHome_DoorLocked()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "unlocked");

        Context.GetApp<FrontDoorSecurity>();
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        HaMock.Verify(x => x.CallService("lock", "lock", 
            It.IsAny<ServiceTarget>(),
            It.IsAny<LockLockParameters>()), Times.Once);
        
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "locked");
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
        HaMock.Verify(x => x.CallService("notify", "family", null,
            It.Is<NotifyFamilyParameters>(y => y.Message!.Contains("Locked front door."))), Times.Once);
    }
    
    [Test]
    public void FrontDoorSecurity_FailedToLockDoor_FamilyNotified()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "unlocked");

        Context.GetApp<FrontDoorSecurity>();
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        HaMock.Verify(x => x.CallService("lock", "lock", 
            It.IsAny<ServiceTarget>(),
            It.IsAny<LockLockParameters>()), Times.Once);
        
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(10).Ticks);
        HaMock.Verify(x => x.CallService("notify", "family", null,
            It.Is<NotifyFamilyParameters>(y => y.Message!.Contains("Attempted to lock the front door but failed."))), Times.Once);
    }
    
    [Test]
    public void FrontDoorSecurity_DoorAlreadyLocked_NobodyNotified()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Lock.FrontDoorLock, "locked");

        Context.GetApp<FrontDoorSecurity>();
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        HaMock.Verify(x => x.CallService("lock", "lock", 
            It.IsAny<ServiceTarget>(),
            It.IsAny<LockLockParameters>()), Times.Never);
        HaMock.Verify(x => x.CallService("notify", "family", null,
            It.IsAny<NotifyFamilyParameters>()), Times.Never);
    }
}