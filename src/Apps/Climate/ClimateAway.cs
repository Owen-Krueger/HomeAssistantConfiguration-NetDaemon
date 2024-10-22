using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models;
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
        entities.InputSelect.HomeState
            .StateChanges()
            .Subscribe(_ => UpdateAutomationTriggers());
    }
    
    /// <summary>
    /// Updates the automation triggers. If state is "Away", ensures that the triggers are active. If state
    /// is "Home", ensures all triggers are disposed.
    /// </summary>
    private void UpdateAutomationTriggers()
    {
        switch (GetHomeState())
        {
            // Sets up automation triggers.
            case HomeStateEnum.Away when automationTriggers.Count == 0:
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
            case HomeStateEnum.Home when automationTriggers.Count > 0:
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
        var desiredTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        var isHeatMode = entities.Climate.Main.IsHeatMode();

        timingThresholds = ClimateUtilities.GetTimingThresholds(desiredTemperature, isHeatMode);
        logger.LogInformation("Updating timing thresholds. Desired temperature: {Temperature}. Is Heat Mode: {IsHeatMode}. Thresholds: {Thresholds}",
            desiredTemperature, isHeatMode, string.Join(", ", timingThresholds));

        if (GetHomeState() == HomeStateEnum.Away)
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
    /// Gets the currently set temperature on the thermostat.
    /// </summary>
    private double GetSetTemperature()
        => entities.Climate.Main.Attributes?.Temperature ?? 0.0;
    
    /// <summary>
    /// Gets the current state of the house as <see cref="HomeStateEnum"/>.
    /// </summary>
    private HomeStateEnum GetHomeState()
        => entities.InputSelect.HomeState.GetEnumFromState<HomeStateEnum>();
    
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