using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Laundry;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class LaundryTests : TestBase
{
    [Test]
    public void Laundry_WasherFinished_FamilyNotified()
    {
        HaMock.TriggerStateChange(Entities.Sensor.WasherWasherMachineState, "run");

        Context.GetApp<Laundry>();
        HaMock.TriggerStateChange(Entities.Sensor.WasherWasherMachineState, "stop");
        HaMock.Verify(x => x.CallService("notify", "family", null,
            It.Is<NotifyFamilyParameters>(y => y.Message!.Contains("The washer has completed!"))), Times.Once);
    }
    
    [Test]
    public void Laundry_DryerStateFinished_FamilyNotified()
    {
        HaMock.TriggerStateChange(Entities.Sensor.DryerDryerJobState, "cooling");

        Context.GetApp<Laundry>();
        HaMock.TriggerStateChange(Entities.Sensor.DryerDryerJobState, "finished");
        HaMock.Verify(x => x.CallService("notify", "family", null,
            It.Is<NotifyFamilyParameters>(y => y.Message!.Contains("The dryer has completed!"))), Times.Once);
        HaMock.TriggerStateChange(Entities.Sensor.DryerDryerJobState, "cooling");
    }
    
    [Test]
    public void Laundry_DryerStateNone_FamilyNotified()
    {
        HaMock.TriggerStateChange(Entities.Sensor.DryerDryerJobState, "cooling");

        Context.GetApp<Laundry>();
        HaMock.TriggerStateChange(Entities.Sensor.DryerDryerJobState, "none");
        HaMock.Verify(x => x.CallService("notify", "family", null,
            It.Is<NotifyFamilyParameters>(y => y.Message!.Contains("The dryer has completed!"))), Times.Once);
        HaMock.TriggerStateChange(Entities.Sensor.DryerDryerJobState, "cooling");
    }
}