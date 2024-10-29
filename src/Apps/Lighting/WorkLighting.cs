using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Lighting;

/// <summary>
/// Automation for turning on lights around the house on days that Owen is working.
/// </summary>
[NetDaemonApp]
public class WorkLighting
{
    private readonly IEntities entities;
    private readonly IScheduler scheduler;
    private readonly ILogger<WorkLighting> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public WorkLighting(IHaContext context, IScheduler scheduler, ILogger<WorkLighting> logger)
    {
        entities = new Entities(context);
        this.scheduler = scheduler;
        this.logger = logger;

        entities.Switch.OfficeLights
            .StateChanges()
            .Where(x => x.New.IsOff())
            .Subscribe(_ => TurnOnDiningRoomLights());
        entities.BinarySensor.UpstairsTvOn
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromSeconds(10), this.scheduler)
            .Subscribe(_ => TurnOnDownstairsLights());
    }

    /// <summary>
    /// Turns on dining room lights if it appears to be lunchtime on a workday.
    /// </summary>
    private void TurnOnDiningRoomLights()
    {
        if (entities.Switch.DiningRoomLights.IsOn() ||
            entities.InputBoolean.ModeGuest.IsOn() ||
            !entities.Person.Owen.IsHome() ||
            entities.BinarySensor.WorkdaySensor.IsOff() ||
            !scheduler.Now.IsBetween(new TimeOnly(11, 0), new TimeOnly(13, 30)))
        {
            return;
        }
        
        logger.LogInformation("Turning on dining room lights.");
        entities.Switch.DiningRoomLights.TurnOn();
    }

    /// <summary>
    /// Turn on downstairs lights, if it's in the morning of a work day.
    /// </summary>
    private void TurnOnDownstairsLights()
    {
        if (entities.Light.DownstairsLights.IsOn() ||
            entities.InputBoolean.ModeGuest.IsOn() ||
            !entities.Person.Owen.IsHome() ||
            entities.BinarySensor.WorkdaySensor.IsOff() ||
            !scheduler.Now.IsBetween(new TimeOnly(6, 30), new TimeOnly(7, 30)))
        {
            return;
        }
        
        logger.LogInformation("Turning on downstairs lights.");
        entities.Light.DownstairsLights.TurnOn();
    }
}