using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Work;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class WorkTests : TestBase
{
    [Test]
    public void Work_BackFromWalk_ComputerTurnedOn()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 1, 1, 8, 0, 0));
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.BinarySensor.OwenComputerActive, "off");
        HaMock.TriggerStateChange(Entities.BinarySensor.WorkdaySensor, "on");

        Context.GetApp<Work>();
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "off");
        HaMock.Verify(x => x.CallService("button", "press",
            It.IsAny<ServiceTarget>(), null), Times.Once);
        HaMock.TriggerStateChange(Entities.BinarySensor.OwenComputerActive, "on");
        TestScheduler.AdvanceBy(TimeSpan.FromSeconds(30).Ticks);
        
        HaMock.Verify(x => x.CallService("notify", "owen", null,
            It.Is<NotifyOwenParameters>(y => y.Message!.Contains("Computer turned on."))), Times.Once);
    }
    
    [Test]
    public void Work_NotStateToTurnOnComputer_ComputerNotTurnedOn()
    {
        TestScheduler.AdvanceTo(new DateTime(2024, 1, 1, 8, 0, 0));
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.BinarySensor.OwenComputerActive, "off");
        HaMock.TriggerStateChange(Entities.BinarySensor.WorkdaySensor, "off");

        Context.GetApp<Work>();
        // Not workday
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "off");
        
        // Computer on
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.BinarySensor.WorkdaySensor, "on");
        HaMock.TriggerStateChange(Entities.BinarySensor.OwenComputerActive, "on");
        
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "off");
        
        // Not correct time
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "on");
        HaMock.TriggerStateChange(Entities.BinarySensor.OwenComputerActive, "off");
        TestScheduler.AdvanceBy(TimeSpan.FromHours(1).Ticks);
        
        HaMock.TriggerStateChange(Entities.InputBoolean.OwenOnMorningWalk, "off");
        
        HaMock.Verify(x => x.CallService("button", "press",
            It.IsAny<ServiceTarget>(), null), Times.Never);
    }
}