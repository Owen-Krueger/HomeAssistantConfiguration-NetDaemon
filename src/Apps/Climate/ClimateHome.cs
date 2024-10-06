using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Utilities;

namespace HomeAssistantGenerated.Apps.Climate;

/// <summary>
/// Automations for climate.
/// </summary>
[NetDaemonApp]
public class ClimateHome
{
    private readonly IEntities entities;
    private readonly ILogger<ClimateHome> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public ClimateHome(IHaContext context, IScheduler scheduler, ILogger<ClimateHome> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        //scheduler.ScheduleCron("0 6 * * *", SetDayTemperature);
        //scheduler.ScheduleCron("0 21 * * *", SetNightTemperature);
    }

    /// <summary>
    /// Sets thermostat to day temperature.
    /// </summary>
    private void SetDayTemperature()
        => SetTemperature(true);

    /// <summary>
    /// Sets thermostat to night temperature.
    /// </summary>
    private void SetNightTemperature()
        => SetTemperature(false);

    /// <summary>
    /// Set temperature based on the input. If not day, sets to desired temperature, minus the offset.
    /// </summary>
    private void SetTemperature(bool isDay)
    {
        // If not home, the away automations will cover setting temp.
        if (entities.InputSelect.ThermostatState.GetEnumFromState<ThermostatState>() != ThermostatState.Home)
        {
            return;
        }

        var setTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        if (!isDay)
        {
            setTemperature -= entities.InputNumber.ClimateNightOffset.State ?? 0;
        }

        logger.LogInformation("Setting temperature (Old: {Old}) (New: {New}", entities.Climate.Main.Attributes.Temperature, setTemperature);
        entities.Climate.Main.SetTemperature(setTemperature);

        if (!isDay)
        {
            TurnOnBedroomFan(setTemperature);
        }
    }

    /// <summary>
    /// Turn on bedroom fan, if the bedroom is warmer than the set temperature.
    /// </summary>
    private void TurnOnBedroomFan(double setTemperature)
    {
        if (entities.Sensor.BedroomTemperatureSensorTemperature.State <= setTemperature)
        {
            return;
        }

        logger.LogInformation("Turning on bedroom fan.");
        entities.Switch.BedroomFan.TurnOn();
    }
}