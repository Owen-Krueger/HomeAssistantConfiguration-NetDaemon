using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Internet;
using NetDaemon.Extensions;
using NetDaemon.Models.Enums;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Internet;

public class MobileDataTests : TestBase
{
    [Test]
    public void MobileData_OnCellularDataWhileHome_Notified()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Home.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Sensor.OwenPhoneNetworkType, "wifi");

        Context.GetApp<MobileData>();
        HaMock.TriggerStateChange(Entities.Sensor.OwenPhoneNetworkType, "cellular");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(30).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Your phone is currently connected to cellular data."))), Times.Once);
    }
    
    [Test]
    public void MobileData_OnCellularDataAfterGettingHome_Notified()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Away.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Sensor.OwenPhoneNetworkType, "cellular");

        Context.GetApp<MobileData>();
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Home.ToStringLowerCase());
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(30).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Your phone is currently connected to cellular data."))), Times.Once);
    }
    
    [Test]
    public void MobileData_OnCellularDataNotHomeHome_NotNotified()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Away.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Sensor.OwenPhoneNetworkType, "wifi");

        Context.GetApp<MobileData>();
        HaMock.TriggerStateChange(Entities.Sensor.OwenPhoneNetworkType, "cellular");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(30).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.IsAny<NotifyOwenParameters>()), Times.Never);
    }
}