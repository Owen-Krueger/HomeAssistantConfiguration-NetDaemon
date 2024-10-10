using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Walk;

/// <summary>
/// Automations to see if Owen is actively on a walk.
/// </summary>
[NetDaemonApp]
public class Walk
{
    private readonly IEntities entities;
    private readonly IScheduler scheduler;
    private readonly ILogger<Walk> logger;
    
    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public Walk(IHaContext context, IScheduler scheduler, ILogger<Walk> logger)
    {
        entities = new Entities(context);
        this.scheduler = scheduler;
        this.logger = logger;

        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOnWalkBoolean());
        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOffWalkBoolean());
        entities.Lock.FrontDoorLock
            .StateChanges()
            .Where(x => !x.New.IsLocked())
            .Subscribe(_ => TurnOffWalkBoolean());
        entities.Cover.PrimaryGarageDoor
            .StateChanges()
            .Where(x => x.New?.State == "closed")
            .Subscribe(_ => TurnOffWalkBoolean());
        // I shouldn't be on a walk after 9, so turn off the boolean if it wasn't already turned off.
        this.scheduler.ScheduleCron("0 9 * * *", () => SetWalkBooleanState(false));
    }

    /// <summary>
    /// Turn on walk boolean, if it's about the time to be on a walk, and it's a workday.
    /// </summary>
    private void TurnOnWalkBoolean()
    {
        if (entities.BinarySensor.WorkdaySensor.IsOff() ||
            entities.Sensor.OwenPhoneNetworkType.State != "cellular" ||
            !IsMorningWalkTime())
        {
            return;
        }

        SetWalkBooleanState(true);
    }

    /// <summary>
    /// Turns off walk boolean, if it seems like Owen is getting back from a walk.
    /// </summary>
    private void TurnOffWalkBoolean()
    {
        if (!IsMorningWalkTime() ||
            entities.Sensor.OwenDistanceMiles.State > 0)
        {
            return;
        }
        
        SetWalkBooleanState(false);
    }

    /// <summary>
    /// Updates the walk boolean to the requested value.
    /// </summary>
    private void SetWalkBooleanState(bool isOnWalk)
    {
        if (entities.InputBoolean.OwenOnMorningWalk.IsOn() == isOnWalk)
        {
            return;
        }
        
        logger.LogInformation("Setting morning walk boolean to {state}", isOnWalk.GetOnOffString());
        if (isOnWalk)
        {
            entities.InputBoolean.OwenOnMorningWalk.TurnOn();
            return;
        }
        
        entities.InputBoolean.OwenOnMorningWalk.TurnOff();
    }

    /// <summary>
    /// Returns if it's around when Owen walks in the morning.
    /// </summary>
    private bool IsMorningWalkTime()
        => scheduler.Now.IsBetween(new TimeOnly(7, 30), new TimeOnly(8, 30));
}