using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Work;

/// <summary>
/// Work automations.
/// </summary>
[NetDaemonApp]
public class Work
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly IScheduler scheduler;
    private readonly ILogger<Work> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public Work(IHaContext context, IScheduler scheduler, ILogger<Work> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.scheduler = scheduler;
        this.logger = logger;

        entities.InputBoolean.OwenOnMorningWalk
            .StateChanges()
            .Where(x => x.New.IsOff())
            .Subscribe(_ => TurnOnComputer());
    }

    /// <summary>
    /// Turns on Owen's computer, if it appears to be the start of the work day.
    /// </summary>
    private void TurnOnComputer()
    {
        if (entities.BinarySensor.OwenComputerActive.IsOn() ||
            entities.BinarySensor.WorkdaySensor.IsOff() ||
            !scheduler.Now.IsBetween(new TimeOnly(7, 30), new TimeOnly(8, 30)))
        {
            return;
        }
        
        logger.LogInformation("Turning on Owen's computer.");
        services.Button.Press(ServiceTarget.FromEntity(entities.Button.WakeOnLanOwenDesktop.EntityId));
        scheduler.Schedule(TimeSpan.FromSeconds(30), VerifyComputerOn);
    }

    /// <summary>
    /// Notifies Owen if his computer was turned on or not.
    /// </summary>
    private void VerifyComputerOn()
    {
        logger.LogInformation("Computer state: {State}", entities.BinarySensor.OwenComputerActive.State);
        services.Notify.Owen(entities.Lock.FrontDoorLock.IsLocked() ? 
                "Computer turned on." : "Attempted to turn on computer but failed.",
            "Computer");    
    }
}