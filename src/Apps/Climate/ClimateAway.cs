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
public class ClimateAway //: IAsyncInitializable
{
    private readonly IEntities entities;
    private readonly IServices services;
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
        this.logger = logger;
        
        UpdateAutomationTriggers();
        UpdateTimingThresholds();
        entities.InputSelect.ThermostatState
            .StateChanges()
            .Subscribe(_ => UpdateAutomationTriggers());
    }
    
    // /// <summary>
    // /// Sets the timing thresholds when the class is initialized for the day.
    // /// </summary>
    // public async Task InitializeAsync(CancellationToken cancellationToken)
    // {
    //     if (GetThermostatState() != ThermostatState.Away)
    //     {
    //         return;
    //     }
    //     
    //     await UpdateTimingThresholds();
    // }
    
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
        UpdateSetTemperature();
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void UpdateSetTemperature()
    {
        // This isn't exact, but adding 10 to the distance of whoever is closest is usually pretty close.
        var minutesFromHome = new List<double>
        {
            entities.Sensor.AllisonDistanceMiles.State ?? 0, 
            entities.Sensor.OwenDistanceMiles.State ?? 0
        }.Min() + 10;

        // The first threshold we're below represents the new temperature we should set.
        foreach (var timing in timingThresholds.Where(timing => minutesFromHome < timing.MinutesToDesired))
        {
            SetTemperature(timing.Temperature);
            return;
        }
    }

    /// <summary>
    /// Sets the temperature on the thermostat.
    /// </summary>
    private void SetTemperature(double temperature)
    {
        var currentTemperature = GetSetTemperature();
        if (Math.Abs(currentTemperature - temperature) < 1)
        {
            return;
        }
        
        entities.Climate.Main.SetTemperature(temperature);
        logger.LogInformation("Setting temperature (Old: {Old}) (New: {New})", currentTemperature, temperature);
        NotifyTemperatureUpdate(temperature);
    }
    
    // /// <summary>
    // /// Gets today's forecast from OpenWeather's API and gets the high/low temperature for today.
    // /// </summary>
    // private async Task UpdateTimingThresholds()
    // {
    //     logger.LogInformation("Getting high/min temperature for today...");
    //     var forecastResult = await entities.Weather.Openweathermap.GetForecastsAsync(type: "hourly");
    //     var forecasts = forecastResult?
    //         .GetProperty(entities.Weather.Openweathermap.EntityId)
    //         .GetProperty("forecast");
    //     if (forecasts is null)
    //     {
    //         logger.LogWarning("Failed to get forecasts from OpenWeatherMap. Using the current temperature.");
    //         SetAutomationState(entities.Weather.Home.Attributes?.Temperature ?? 0);
    //         return;
    //     }
    //
    //     var forecastItems = forecasts.Value.Deserialize<List<WeatherForecastItem>>();
    //     if (forecastItems is null)
    //     {
    //         logger.LogWarning("Failed to deserialize forecast items from `GetForecasts` call. Using the current temperature.");
    //         SetAutomationState(entities.Weather.Home.Attributes?.Temperature ?? 0);
    //         return;
    //     }
    //
    //     var temperatures = forecastItems
    //         .Where(x => x.DateTime.ToUsCentralTime().Date == DateTime.Today)
    //         .Select(x => x.Temperature);
    //
    //     SetAutomationState(entities.Climate.Main.IsHeatMode() switch
    //     {
    //         true => temperatures.Min(),
    //         false => temperatures.Max()
    //     });
    // }

    // private void SetAutomationState(double temperature)
    // {
    //     state = new ThermostatAwayState(temperature, GetTimingThresholds());
    // }


    /// <summary>
    /// Gets the thresholds
    /// </summary>
    /// <returns></returns>
    private List<TimingThreshold> GetTimingThresholds()
    {
        List<TimingThreshold> temperatureTimes = [];
        var desiredTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        var factor = entities.Climate.Main.IsHeatMode() ? -1 : 1;

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