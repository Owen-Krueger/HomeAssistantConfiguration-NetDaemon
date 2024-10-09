using HomeAssistantGenerated;
using Moq;
using NetDaemon.Apps.Climate;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models.Climate;
using NetDaemon.Tests.TestHelpers;

namespace NetDaemon.Tests.Apps;

public class ClimateHomeTests : TestBase
{
    // [Test]
    // public async Task ClimateHome_VariousTimesOfDay_TemperatureSet()
    // {
    //     const int dayTemperature = 71;
    //     const int nightOffset = 2;
    //     var day = new DateTimeOffset(2024, 01, 01, 5, 0, 0, TimeZoneInfo.Local.BaseUtcOffset);
    //     TestScheduler.AdvanceTo(day.Ticks);
    //     HaMock.TriggerStateChange(Entities.Climate.Main, "cool", new ClimateAttributes { Temperature = 60 });
    //     HaMock.TriggerStateChange(Entities.InputSelect.ThermostatState, ThermostatState.Home.ToString());
    //     HaMock.TriggerStateChange(Entities.InputNumber.ClimateDayTemp, dayTemperature.ToString());
    //     HaMock.TriggerStateChange(Entities.InputNumber.ClimateNightOffset, nightOffset.ToString());
    //
    //     Context.GetApp<ClimateHome>();
    //     HaMock.Verify(x => x.CallService("climate", "set_temperature",
    //             It.IsAny<ServiceTarget>(),
    //             It.Is<ClimateSetTemperatureParameters>(y => (y.Temperature ?? 0).Equals(dayTemperature - nightOffset))),
    //         Times.Once());
    //     
    //     TestScheduler.AdvanceTo(day.AddSeconds(3599).Ticks);
    //     TestScheduler.Start();
    //     await Task.Delay(TimeSpan.FromSeconds(1));
    //     TestScheduler.Stop();
    //     HaMock.Verify(x => x.CallService("climate", "set_temperature",
    //         It.IsAny<ServiceTarget>(),
    //         It.Is<ClimateSetTemperatureParameters>(y => (y.Temperature ?? 0).Equals(dayTemperature))),
    //         Times.Once());
    //     
    //     // TestScheduler.AdvanceBy(TimeSpan.FromHours(15).Ticks);
    //     // HaMock.Verify(x => x.CallService("climate", "set_temperature",
    //     //         It.IsAny<ServiceTarget>(),
    //     //         It.Is<ClimateSetTemperatureParameters>(y => (y.Temperature ?? 0).Equals(dayTemperature - nightOffset))),
    //     //     Times.Once());
    // }
}