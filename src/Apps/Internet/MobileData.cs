using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Internet;

/// <summary>
/// Automations for phone mobile data.
/// </summary>
[NetDaemonApp]
public class MobileData
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly ILogger<MobileData> logger;
    
    /// <summary>
    /// Sets up automations.
    /// </summary>
    public MobileData(IHaContext context, IScheduler scheduler, ILogger<MobileData> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.logger = logger;

        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => x.IsHome(), TimeSpan.FromMinutes(30), scheduler)
            .Subscribe(_ => NotifyOwen());
        entities.Sensor.OwenPhoneNetworkType
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "cellular", TimeSpan.FromMinutes(30), scheduler)
            .Subscribe(_ => NotifyOwen());
    }

    /// <summary>
    /// Notifies Owen if he's home without Wi-Fi on.
    /// </summary>
    private void NotifyOwen()
    {
        if (!entities.Person.Owen.IsHome() || entities.Sensor.OwenPhoneNetworkType.State != "cellular")
        {
            return;
        }
        
        logger.LogInformation("Notifying Owen that he's home with cellular on.");
        services.Notify.Owen("Your phone is currently connected to cellular data.", "Phone");
    }
}