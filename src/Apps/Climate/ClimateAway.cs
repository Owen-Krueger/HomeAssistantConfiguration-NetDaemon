using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Models.Climate;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Climate;

/// <summary>
/// Automations for climate when away.
/// </summary>
[NetDaemonApp]
public class ClimateAway : IAsyncInitializable
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
        entities.InputSelect.ThermostatState
            .StateChanges()
            .Subscribe(_ => UpdateAutomationTriggers());
    }
    
    /// <summary>
    /// Sets the timing thresholds when the class is initialized for the day.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (GetThermostatState() != ThermostatState.Away)
        {
            return;
        }
        
        await UpdateTimingThresholds();
    }
    
    /// <summary>
    /// Updates the automation triggers. If state is "Away", ensures that the triggers are active. If state
    /// is "Home", ensures all triggers are disposed.
    /// </summary>
    private void UpdateAutomationTriggers()
    {
        switch (entities.InputSelect.ThermostatState.GetEnumFromState<ThermostatState>())
        {
            // Sets up automation triggers.
            case ThermostatState.Away when automationTriggers.Count == 0:
                break;
            // Removes any existing automation triggers.
            case ThermostatState.Home when automationTriggers.Count > 0:
                automationTriggers = automationTriggers.DisposeTriggers();
                break;
        }
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

    private void SetAutomationState(double temperature)
    {
        state = new ThermostatAwayState(temperature, GetTimingThresholds());
    }

    /// <summary>
    /// Gets the thresholds
    /// </summary>
    /// <returns></returns>
    private List<(double Temperature, double MinutesToDesired)> GetTimingThresholds()
    {
        List<(double, double)> temperatureTimes = [];
        var desiredTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        var factor = entities.Climate.Main.IsHeatMode() ? -1 : 1;

        for (var i = 1; i <= 8; i++)
        {
            var temperature = desiredTemperature + factor * i;
            temperatureTimes.Add((temperature, GetTemperatureTiming(temperature)));
        }

        return temperatureTimes;
    }
    
    /// <summary>
    /// Gets amount of time to arrive at the desired temperature based on how far away the temperature currently is
    /// (the offset).
    /// </summary>
    private double GetTemperatureTiming(double temperature)
    {
        var desiredTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        const int houseVolume = 23_562; // 2,618 liveable x 9 ft ceilings
        const double airDensity = 0.075;
        const double heatOfAir = 0.24;

        if (entities.Climate.Main.IsHeatMode())
        {
            const int heatingCapacity = 60_000;
            return houseVolume * (desiredTemperature - temperature) * airDensity * heatOfAir / heatingCapacity;
        }
        
        const int coolingCapacity = 24_000;
        return houseVolume * (temperature - desiredTemperature) * airDensity * heatOfAir / coolingCapacity;
    }
    
    /// <summary>
    /// Gets the current state of the house as <see cref="ThermostatState"/>.
    /// </summary>
    private ThermostatState GetThermostatState()
        => entities.InputSelect.ThermostatState.GetEnumFromState<ThermostatState>();
}