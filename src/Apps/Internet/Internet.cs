using System.Reactive.Concurrency;
using System.Threading.Tasks;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.apps.Internet;

/// <summary>
/// Automations for internet.
/// </summary>
[NetDaemonApp]
public class Internet
{
    private readonly IEntities entities;
    private readonly ILogger<Internet> logger;
    private readonly IScheduler scheduler;
    
    /// <summary>
    /// SeTs up internet automations.
    /// </summary>
    public Internet(IHaContext context, IScheduler scheduler, ILogger<Internet> logger)
    {
        entities = new Entities(context);
        this.logger = logger;
        this.scheduler = scheduler;

        entities.BinarySensor.InternetUp
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromSeconds(90), scheduler)
            .Subscribe(_ => RestartModem());
    }

    /// <summary>
    /// Attempts to restart the internet modem. This is done by turning off the modem, waiting 15 seconds, and then
    /// turning the modem back on.
    /// </summary>
    private void RestartModem()
    {
        var modemSmartPlug = entities.Switch.InternetModemSmartPlug;
        if (modemSmartPlug.EntityState?.LastChanged > DateTime.Now.AddMinutes(-5))
        {
            logger.LogInformation("Modem smart plug already manually restarted. Not restarting");
            return;
        }
        
        logger.LogInformation("Restarting modem smart plug.");
        modemSmartPlug.TurnOff();
        scheduler.Schedule(DateTimeOffset.Now.AddSeconds(15), TurnOnModemSmartPlug);
    }

    private void TurnOnModemSmartPlug()
        => entities.Switch.InternetModemSmartPlug.TurnOn();
}