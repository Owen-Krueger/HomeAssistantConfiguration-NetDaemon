using System.Reactive.Concurrency;
using System.Threading.Tasks;
using HomeAssistantGenerated;
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
    
    /// <summary>
    /// SeTs up internet automations.
    /// </summary>
    public Internet(IHaContext context, IScheduler scheduler, ILogger<Internet> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        entities.BinarySensor.InternetUp
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromSeconds(90), scheduler)
            .SubscribeAsync(async _ => await RestartModemAsync(), e => 
                logger.LogError(e, "Exception thrown while restarting modem."));
    }

    /// <summary>
    /// Attempts to restart the internet modem. This is done by turning off the modem, waiting 15 seconds, and then
    /// turning the modem back on.
    /// </summary>
    private async Task RestartModemAsync()
    {
        var modemSmartPlug = entities.Switch.InternetModemSmartPlug;
        if (modemSmartPlug.EntityState?.LastChanged > DateTime.Now.AddMinutes(-5))
        {
            logger.LogInformation("{Entity} already manually restarted. Not restarting", modemSmartPlug.EntityId);
            return;
        }
        
        logger.LogInformation("Restarting {Entity}.", modemSmartPlug.EntityId);
        modemSmartPlug.TurnOff();
        await Task.Delay(TimeSpan.FromSeconds(15));
        modemSmartPlug.TurnOn();
    }
}