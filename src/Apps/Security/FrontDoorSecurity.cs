using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Security;

/// <summary>
/// Automations for the lock on the front door.
/// </summary>
[NetDaemonApp]
public class FrontDoorSecurity
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly IScheduler scheduler;
    private readonly ILogger<FrontDoorSecurity> logger;
    private DateTimeOffset lastExecution;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public FrontDoorSecurity(IHaContext context, IScheduler scheduler, ILogger<FrontDoorSecurity> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.scheduler = scheduler;
        this.logger = logger;
        lastExecution = scheduler.Now.AddDays(-1);

        entities.Person.Owen
            .StateChanges()
            .Where(x => !x.New.IsHome())
            .Subscribe(_ => LockFrontDoor());
        entities.Person.Allison
            .StateChanges()
            .Where(x => !x.New.IsHome())
            .Subscribe(_ => LockFrontDoor());
        scheduler.ScheduleCron("30 21 * * *", LockFrontDoor);
    }

    /// <summary>
    /// Locks the front door if nobody is home, and it's currently unlocked.
    /// </summary>
    private void LockFrontDoor()
    {
        if (entities.IsAnyoneHome() || entities.Lock.FrontDoorLock.IsLocked() ||
            lastExecution >= scheduler.Now.AddMinutes(-5)) // To prevent two notifications if Owen and Allison are traveling together.
        {
            return;
        }
        
        logger.LogInformation("Locking front door.");
        lastExecution = scheduler.Now;
        entities.Lock.FrontDoorLock.Lock();
        scheduler.Schedule(scheduler.Now.AddSeconds(10), VerifyFrontDoorLocked);
    }

    /// <summary>
    /// Notify if we were able to lock the front door or not.
    /// </summary>
    private void VerifyFrontDoorLocked()
    {
        logger.LogInformation("Front door state: {State}", entities.Lock.FrontDoorLock.State);
        services.Notify.Family(entities.Lock.FrontDoorLock.IsLocked() ? 
            "Locked front door." : "Attempted to lock the front door but failed.",
            "Front Door");
    }
}