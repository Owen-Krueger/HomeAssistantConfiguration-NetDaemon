using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models.Climate;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Climate;

/// <summary>
/// Automations for climate when away.
/// </summary>
[NetDaemonApp]
public class ClimateAway
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly IScheduler scheduler;
    private readonly ILogger<ClimateAway> logger;
    private List<IDisposable> automationTriggers = [];
    private List<TimingThreshold> timingThresholds = [];

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public ClimateAway(IHaContext context, IScheduler scheduler, ILogger<ClimateAway> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.scheduler = scheduler;
        this.logger = logger;
        
        UpdateAutomationTriggers();
        UpdateTimingThresholds();
        entities.InputSelect.ThermostatState
            .StateChanges()
            .Subscribe(_ => UpdateAutomationTriggers());
    }
    
    /// <summary>
    /// Updates the automation triggers. If state is "Away", ensures that the triggers are active. If state
    /// is "Home", ensures all triggers are disposed.
    /// </summary>
    private void UpdateAutomationTriggers()
    {
        switch (GetThermostatState())
        {
            // Sets up automation triggers.
            case ThermostatState.Away when automationTriggers.Count == 0:
                logger.LogInformation("Climate Away automations enabled.");
                automationTriggers.Add(entities.InputNumber.ClimateDayTemp
                    .StateChanges()
                    .WhenStateIsFor(_ => true, TimeSpan.FromSeconds(15), scheduler)
                    .Subscribe(_ => UpdateTimingThresholds()));
                automationTriggers.Add(entities.Sensor.AllisonDistanceMiles
                    .StateChanges()
                    .Subscribe(_ => UpdateSetTemperature()));
                automationTriggers.Add(entities.Sensor.OwenDistanceMiles
                    .StateChanges()
                    .Subscribe(_ => UpdateSetTemperature()));
                break;
            // Removes any existing automation triggers.
            case ThermostatState.Home when automationTriggers.Count > 0:
                logger.LogInformation("Climate Away automations disabled.");
                automationTriggers = automationTriggers.DisposeTriggers();
                break;
        }
    }

    /// <summary>
    /// Updates the <see cref="timingThresholds"/> variable. Also sets the thermostat temperature, if it needs
    /// updating.
    /// </summary>
    private void UpdateTimingThresholds()
    {
        timingThresholds = GetTimingThresholds();

        if (GetThermostatState() == ThermostatState.Away)
        {
            UpdateSetTemperature();
        }
    }
    
    /// <summary>
    /// Updates the set temperature, based on how close someone is to home.
    /// </summary>
    private void UpdateSetTemperature()
    {
        // This isn't exact, but adding 10 to the distance of whoever is closest is usually pretty close.
        var minutesFromHome = new List<double>
        {
            entities.Sensor.AllisonDistanceMiles.State ?? 0, 
            entities.Sensor.OwenDistanceMiles.State ?? 0
        }.Min() + 10;

        var temperature = 70.0;
        foreach (var timing in timingThresholds)
        {
            // If someone's minutes from home is under the threshold, we want to set the temperature to be the
            // temperature of the previous threshold.
            if (minutesFromHome < timing.MinutesToDesired)
            {
                SetTemperature(temperature);
                return;
            }

            temperature = timing.Temperature;
        }
        
        SetTemperature(temperature);
    }

    /// <summary>
    /// Sets the temperature on the thermostat.
    /// </summary>
    private void SetTemperature(double temperature)
    {
        var currentTemperature = GetSetTemperature();
        if (currentTemperature.Equals(temperature))
        {
            return;
        }
        
        entities.Climate.Main.SetTemperature(temperature);
        logger.LogInformation("Setting temperature (Old: {Old}) (New: {New})", currentTemperature, temperature);
        NotifyTemperatureUpdate(temperature);
    }
    
    /// <summary>
    /// Gets the thresholds
    /// </summary>
    private List<TimingThreshold> GetTimingThresholds()
    {
        List<TimingThreshold> temperatureTimes = [];
        var desiredTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        var factor = entities.Climate.Main.IsHeatMode() ? -1 : 1;

        temperatureTimes.Add(new TimingThreshold(desiredTemperature, 0));
        for (var i = 1; i <= 8; i++)
        {
            var temperature = desiredTemperature + factor * i;
            temperatureTimes.Add(GetTemperatureTiming(temperature));
        }

        // These should already be in order, but I'm paranoid.
        temperatureTimes = temperatureTimes.OrderBy(x => x.MinutesToDesired).ToList();
        
        logger.LogInformation("Temperature timings updated: {Timings}", 
            string.Join(',', temperatureTimes.Select(x => x.ToString())));

        return temperatureTimes;
    }
    
    /// <summary>
    /// Gets amount of time to arrive at the desired temperature based on how far away the temperature currently is
    /// (the offset).
    /// </summary>
    private TimingThreshold GetTemperatureTiming(double temperature)
    {
        var desiredTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        const int houseVolume = 23_562; // 2,618 liveable x 9 ft ceilings
        const double airDensity = 0.075;
        const double heatOfAir = 0.24;

        if (entities.Climate.Main.IsHeatMode())
        {
            const int heatingCapacity = 60_000;
            return new TimingThreshold(temperature, 
                houseVolume * (desiredTemperature - temperature) * airDensity * heatOfAir / heatingCapacity);
        }
        
        const int coolingCapacity = 24_000;
        return new TimingThreshold(temperature, 
            houseVolume * (temperature - desiredTemperature) * airDensity * heatOfAir / coolingCapacity);
    }

    /// <summary>
    /// Gets the currently set temperature on the thermostat.
    /// </summary>
    private double GetSetTemperature()
        => entities.Climate.Main.Attributes?.Temperature ?? 0.0;
    
    /// <summary>
    /// Gets the current state of the house as <see cref="ThermostatState"/>.
    /// </summary>
    private ThermostatState GetThermostatState()
        => entities.InputSelect.ThermostatState.GetEnumFromState<ThermostatState>();
    
    /// <summary>
    /// Notifies Owen that the set temperature has been updated (if he wants updates).
    /// </summary>
    private void NotifyTemperatureUpdate(double temperature)
    {
        if (entities.InputBoolean.ClimateNotifyLocationBased.IsOff())
        {
            return;
        }
        
        services.Notify.Owen($"Temperature set to {temperature}.", "Climate");
    }
}