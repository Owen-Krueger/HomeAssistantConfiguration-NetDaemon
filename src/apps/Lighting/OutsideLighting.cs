﻿using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations for outside lighting.
/// </summary>
[NetDaemonApp]
public class OutsideLighting
{
    private readonly IEntities entities;
    private readonly ILogger<OutsideLighting> logger;
    
    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public OutsideLighting(IHaContext context, IScheduler scheduler, ILogger<OutsideLighting> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        // Schedule-based automations.
        scheduler.Schedule(GetSunsetTime().AddMinutes(-15), TurnOnPorch);
        scheduler.ScheduleCron("0 22 * * *", TurnOffPorchTimeBased);
        scheduler.ScheduleCron("0 0 * * *", TurnOffPorch); // Turn off the porch at midnight, no matter what.
        
        // Gets the next time to run our sunset script. 1 minute after sunset should have the next
        // sunset time exposed in HomeAssistant.
        scheduler.Schedule(GetSunsetTime().AddMinutes(1), 
            () => scheduler.Schedule(GetSunsetTime(), TurnOnPorch));

        // Location-based automations.
        entities.Sensor.AllisonDistanceMiles
            .StateChanges()
            .Where(x =>
                    x.Old?.State > 5 &&
                    x.New?.State <= 5)
            .Subscribe(_ => TurnOnPorchLocationBased());
        entities.Sensor.OwenDistanceMiles
            .StateChanges()
            .Where(x =>
                x.Old?.State > 5 &&
                x.New?.State <= 5)
            .Subscribe(_ => TurnOnPorchLocationBased());
        entities.Person.Allison
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "home", TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOffPorch());
        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "home", TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOffPorch());
    }

    /// <summary>
    /// Turns on the porch lights.
    /// </summary>
    private void TurnOnPorch() => SetPorchLighting(true);

    /// <summary>
    /// Turns on the porch lights if it's late and the lights are off.
    /// </summary>
    private void TurnOnPorchLocationBased()
    {
        if (DateTime.Now.TimeOfDay > TimeSpan.FromHours(22))
        {
            TurnOnPorch();
        }
    }

    /// <summary>
    /// Turns off the porch lights if someone is almost home.
    /// </summary>
    private void TurnOffPorchTimeBased()
    {
        var owenDistance = entities.Sensor.OwenDistanceMiles.State ?? 0;
        var allisonDistance = entities.Sensor.AllisonDistanceMiles.State ?? 0;

        // If someone is close to home, don't turn off the lights yet.
        if (allisonDistance is > 0 and < 5 && owenDistance is > 0 and < 5)
        {
            logger.LogInformation("Someone getting close to home. Not turning off front porch lights yet.");
            return;
        }
     
        TurnOffPorch();
    }

    /// <summary>
    /// Turns off porch lights if they're actively on.
    /// </summary>
    private void TurnOffPorch()
        => SetPorchLighting(false);

    /// <summary>
    /// Turns on or off the porch and holiday lights.
    /// </summary>
    /// <param name="isOn">Whether to turn on the lights.</param>
    private void SetPorchLighting(bool isOn)
    {
        logger.LogInformation("Turning the porch lights {State}.", isOn ? "on" : "off");
        switch (isOn)
        {
            case true when entities.Switch.FrontPorchLights.IsOff():
                entities.Switch.FrontPorchLights.TurnOn();
                break;
            case false when entities.Switch.FrontPorchLights.IsOn():
                entities.Switch.FrontPorchLights.TurnOff();
                break;
        }

        if (!entities.InputBoolean.HolidayMode.IsOn())
        {
            return;
        }
        
        switch (isOn)
        {
            case true when entities.Group.HolidayLights.IsOff():
                entities.Group.HolidayLights.CallService("homeassistant.turn_on");
                break;
            case false when entities.Group.HolidayLights.IsOn():
                entities.Group.HolidayLights.CallService("homeassistant.turn_off");
                break;
        }
    }

    /// <summary>
    /// Gets the time of the next sunset.
    /// </summary>
    private DateTimeOffset GetSunsetTime()
    {
        if (DateTimeOffset.TryParse(entities.Sensor.SunNextSetting.EntityState?.State ?? string.Empty,
                out var date))
        {
            return date;
        }
        
        logger.LogWarning("Failed to get sunset time from state. Defaulting to 5PM.");
        // Default to 5 PM if we can't pull the date from state.
        return new DateTimeOffset(DateTime.Today.AddHours(17));
    }
}