using System.Text.Json;
using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Security;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.Extensions;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models;
using NetDaemon.Models.Enums;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps.Security;

public class GarageSecurityTests : TestBase
{
    [Test]
    public void GarageSecurity_NobodyHome_NotificationSent()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Away.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Away.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.InputSelect.HomeState, HomeStateEnum.Home.ToString());
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "open");

        Context.GetApp<GarageSecurity>();
        HaMock.TriggerStateChange(Entities.InputSelect.HomeState, HomeStateEnum.Away.ToString());
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
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Home.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Away.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Cover.PrimaryGarageDoor, "closed");

        Context.GetApp<GarageSecurity>();
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Away.ToStringLowerCase());
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(5).Ticks);
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Garage door is open"))), Times.Never);
    }
}