﻿using System.Reactive.Concurrency;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Lighting;

/// <summary>
/// Automations for bedroom lighting.
/// </summary>
[NetDaemonApp]
public class BedroomLighting
{
    private readonly IEntities entities;
    private readonly IScheduler scheduler;
    private readonly ILogger<BedroomLighting> logger;
    
    /// <summary>
    /// Sets up automations.
    /// </summary>
    public BedroomLighting(IHaContext context, IScheduler scheduler, ILogger<BedroomLighting> logger)
    {
        entities = new Entities(context);
        this.scheduler = scheduler;
        this.logger = logger;

        context.Events.Filter<ZhaEvent>(EventTypes.ZhaEvent)
            .Where(x =>
                x.Data is { DeviceId: DeviceIds.BedsideButtonId, Command: "single" })
            .Subscribe(_ => OnBedsideButtonPressed());
        entities.Light.BedroomLamps
            .StateChanges()
            .Where(x => 
                x.Old.IsOff() && 
                x.New.IsOn())
            .Subscribe(_ => ActivateNightLighting());
        entities.Switch.BedroomLights
            .StateChanges()
            .Where(x => 
                x.Old.IsOn() && 
                x.New.IsOff())
            .Subscribe(_ => ActivateNightLighting());
    }

    /// <summary>
    /// When bedside button is pressed, toggle lamps. If it's nighttime and lamps are being turned on, turn
    /// off the bedroom lights.
    /// </summary>
    private void OnBedsideButtonPressed()
    {
        logger.LogInformation("Toggling bedroom lamps. Current state: {State}",
            entities.Light.BedroomLamps.EntityState?.State.GetOnOffStringFromState());
        entities.Light.BedroomLamps.Toggle();

        if (!IsLate() || entities.Switch.BedroomLights.IsOff() || entities.Light.BedroomLamps.IsOff())
        {
            return;
        }
        
        logger.LogInformation("Turning off bedroom lights, due to it being late and lamps are on.");
        entities.Switch.BedroomLights.TurnOff();
    }

    /// <summary>
    /// If it's late, turn on lamps and turn off bedroom lights.
    /// </summary>
    private void ActivateNightLighting()
    {
        if (!IsLate())
        {
            return;
        }

        logger.LogInformation("Turning on lamps and turning off lights, due to it being night-time.");
        if (entities.Light.BedroomLamps.IsOff())
        {
            entities.Light.BedroomLamps.TurnOn();
        }

        if (entities.Switch.BedroomLights.IsOn())
        {
            entities.Switch.BedroomLights.TurnOff();
        }
    }

    /// <summary>
    /// Returns if it's between 9PM and midnight.
    /// </summary>
    private bool IsLate()
        => scheduler.Now.IsBetween(new TimeOnly(21, 00), new TimeOnly(23, 59, 59));
}