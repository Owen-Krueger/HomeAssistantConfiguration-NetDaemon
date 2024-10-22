using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.State;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models.Enums;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class HomeStateTests : TestBase
{
    [Test]
    public void HomeState_EveryoneGone_AwayMode()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.InputSelect.HomeState, HomeStateEnum.Home.ToString());
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Home.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Away.ToStringLowerCase());

        Context.GetApp<HomeState>();
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Away.ToStringLowerCase());
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(15).Ticks);
        
        HaMock.Verify(x => x.CallService("input_select", "select_option",
            It.IsAny<ServiceTarget>(), 
            It.Is<InputSelectSelectOptionParameters>(y => y.Option == HomeStateEnum.Away.ToString())), Times.Once);
    }
    
    [Test]
    public void HomeState_SomeoneHome_HomeMode()
    {
        HaMock.TriggerStateChange(Entities.InputSelect.HomeState, HomeStateEnum.Away.ToString());
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Away.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Away.ToStringLowerCase());

        Context.GetApp<HomeState>();
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Home.ToStringLowerCase());
        
        HaMock.Verify(x => x.CallService("input_select", "select_option",
            It.IsAny<ServiceTarget>(), 
            It.Is<InputSelectSelectOptionParameters>(y => y.Option == HomeStateEnum.Home.ToString())), Times.Once);
    }
    
    [Test]
    public void HomeState_StateAlreadyCorrect_ModeNotUpdated()
    {
        HaMock.TriggerStateChange(Entities.InputSelect.HomeState, HomeStateEnum.Home.ToString());
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Away.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Away.ToStringLowerCase());

        Context.GetApp<HomeState>();
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Home.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.InputSelect.HomeState, HomeStateEnum.Away.ToString());
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Away.ToStringLowerCase());
        
        HaMock.Verify(x => x.CallService("input_select", "select_option",
            It.IsAny<ServiceTarget>(), 
            It.IsAny<InputSelectSelectOptionParameters>()), Times.Never);
    }
    
    [Test]
    public void HomeState_SomeoneHomeAndSomeoneAway_ModeStaysHome()
    {
        TestScheduler.AdvanceToNow();
        HaMock.TriggerStateChange(Entities.InputSelect.HomeState, HomeStateEnum.Home.ToString());
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Home.ToStringLowerCase());
        HaMock.TriggerStateChange(Entities.Person.Owen, PersonStateEnum.Home.ToStringLowerCase());

        Context.GetApp<HomeState>();
        HaMock.TriggerStateChange(Entities.Person.Allison, PersonStateEnum.Away.ToStringLowerCase());
        TestScheduler.AdvanceBy(TimeSpan.FromMinutes(15).Ticks);
        
        HaMock.Verify(x => x.CallService("input_select", "select_option",
            It.IsAny<ServiceTarget>(), 
            It.Is<InputSelectSelectOptionParameters>(y => y.Option == HomeStateEnum.Away.ToString())), Times.Never);
    }
}