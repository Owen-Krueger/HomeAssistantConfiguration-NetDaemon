using System.Text.Json;
using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Security;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Security;

public class GarageSecurityTests : TestBase
{
    [Test]
    public void GarageSecurity_NobodyHome_NotificationSent()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "open");

        Context.GetApp<GarageSecurity>();
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Garage door is open"))), Times.Once);
        var mobileNotificationEvent = new Event
        {
            EventType = EventTypes.MobileAppNotificationActionEvent,
            DataElement = JsonSerializer.SerializeToElement(new MobileNotificationActionEvent { Action = "CLOSE_GARAGE_DOOR" })
        };
        HaMock.TriggerEvent(mobileNotificationEvent);
        HaMock.Verify(x => x.CallService("cover", "close_cover", It.IsAny<ServiceTarget>(),
            It.IsAny<object>()), Times.Once);
    }

    [Test]
    public void GarageSecurity_GarageDoorShut_NoNotificationSent()
    {
        TestScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
        HaMock.TriggerStateChange(Entities.Person.Allison, "home");
        HaMock.TriggerStateChange(Entities.Person.Owen, "away");
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");

        Context.GetApp<GarageSecurity>();
        HaMock.TriggerStateChange(Entities.Person.Allison, "away");
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Garage door is open"))), Times.Never);
    }
}