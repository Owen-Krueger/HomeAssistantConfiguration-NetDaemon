﻿using System.Collections.Generic;
using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models;
using NetDaemon.Models.Enums;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Climate;

/// <summary>
/// Automations for climate when home.
/// </summary>
[NetDaemonApp]
public class ClimateHome
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly IScheduler scheduler;
    private readonly ILogger<ClimateHome> logger;
    private readonly List<IDisposable> automationTriggers = [];

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public ClimateHome(IHaContext context, IScheduler scheduler, ILogger<ClimateHome> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.scheduler = scheduler;
        this.logger = logger;
        TriggerUtilities.UpdateAutomationTriggers(automationTriggers,
            entities.InputSelect.HomeState.GetEnumFromState<HomeStateEnum>() == HomeStateEnum.Home,
            GetAutomationTriggers);

        entities.InputSelect.HomeState
            .StateChanges()
            .Subscribe(x => 
                TriggerUtilities.UpdateAutomationTriggers(automationTriggers,
                    x.New.GetEnumFromState<HomeStateEnum>() == HomeStateEnum.Home, GetAutomationTriggers));
    }

    /// <summary>
    /// Sets up all automation triggers and sets temperature.
    /// </summary>
    private List<IDisposable> GetAutomationTriggers()
    {
        logger.LogInformation("Climate Home automations enabled.");
        UpdateSetTemperature(scheduler.Now.IsBetween(new TimeOnly(6, 0), new TimeOnly(21, 0)));
        return
        [
            scheduler.ScheduleCron("0 6 * * *", SetDayTemperature),
            scheduler.ScheduleCron("0 21 * * *", SetNightTemperature)
        ];
    }

    /// <summary>
    /// Sets thermostat to day temperature.
    /// </summary>
    private void SetDayTemperature()
        => UpdateSetTemperature(true);

    /// <summary>
    /// Sets thermostat to night temperature.
    /// </summary>
    private void SetNightTemperature()
        => UpdateSetTemperature(false);

    /// <summary>
    /// Updates the thermometer's set temperature. 
    /// </summary>
    private void UpdateSetTemperature(bool isDay)
    {
        // If not home, the away automations will cover setting temp.
        if (entities.InputSelect.HomeState.GetEnumFromState<HomeStateEnum>() != HomeStateEnum.Home)
        {
            return;
        }
        
        var setTemperature = entities.InputNumber.ClimateDayTemp.State ?? 70;
        if (!isDay)
        {
            setTemperature -= entities.InputNumber.ClimateNightOffset.State ?? 0;
        }

        SetTemperature(setTemperature);
        
        if (!isDay)
        {
            TurnOnBedroomFan(setTemperature);
        }
    }

    /// <summary>
    /// Set temperature based on the input. If not day, sets to desired temperature, minus the offset.
    /// </summary>
    private void SetTemperature(double setTemperature)
    {
        var currentSetTemperature = entities.Climate.Main.Attributes?.Temperature;
        if (currentSetTemperature is null || setTemperature.Equals(currentSetTemperature))
        {
            return;
        }

        logger.LogInformation("Setting temperature (Old: {Old}) (New: {New})", 
            entities.Climate.Main.Attributes?.Temperature, setTemperature);
        NotifyTemperatureUpdate(setTemperature);
        
        entities.Climate.Main.SetTemperature(setTemperature);
    }

    /// <summary>
    /// Turn on bedroom fan, if the bedroom is warmer than the set temperature.
    /// </summary>
    private void TurnOnBedroomFan(double setTemperature)
    {
        var bedroomTemperature = entities.Sensor.BedroomTemperatureSensorTemperature.State; 
        if (bedroomTemperature <= setTemperature)
        {
            return;
        }

        logger.LogInformation("Turning on bedroom fan. (Current Temp: {CurrentTemp} Set Temp: {SetTemp})",
            bedroomTemperature, setTemperature);
        entities.Switch.BedroomFan.TurnOn();
    }

    /// <summary>
    /// Notifies Owen that the set temperature has been updated (if he wants updates).
    /// </summary>
    private void NotifyTemperatureUpdate(double temperature)
    {
        if (entities.InputBoolean.ClimateNotifyTimeBased.IsOff())
        {
            return;
        }
        
        services.Notify.Owen($"Temperature set to {temperature}.", "Climate");
    }
}