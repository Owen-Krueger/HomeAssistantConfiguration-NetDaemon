using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Internet;

/// <summary>
/// Automations for internet.
/// </summary>
[NetDaemonApp]
public class Internet
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly ILogger<Internet> logger;
    private readonly IScheduler scheduler;
    
    /// <summary>
    /// Sets up automations.
    /// </summary>
    public Internet(IHaContext context, IScheduler scheduler, ILogger<Internet> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
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
        
        logger.LogInformation("Restarting modem smart plug. Starting by turning modem off for 15 seconds.");
        modemSmartPlug.TurnOff();
        scheduler.Schedule(DateTimeOffset.Now.AddSeconds(15), TurnOnModemSmartPlug);
    }

    /// <summary>
    /// Turns modem back on.
    /// </summary>
    private void TurnOnModemSmartPlug()
    {
        logger.LogInformation("Turning modem back on.");
        entities.Switch.InternetModemSmartPlug.TurnOn();
        scheduler.Schedule(DateTimeOffset.Now.AddMinutes(3), VerifyInternetWorking);
    }

    /// <summary>
    /// Verifies if the internet is working again. Notifies Owen if still down.
    /// </summary>
    private void VerifyInternetWorking()
    {
        logger.LogInformation("Internet state: {State}", 
            entities.BinarySensor.InternetUp.State.GetOnOffStringFromState());
        if (entities.BinarySensor.InternetUp.IsOff())
        {
            services.Notify.Owen("Internet still down after modem restart.", "Internet");
        }
    }
}