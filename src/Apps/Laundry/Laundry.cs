namespace NetDaemon.apps.Laundry;

/// <summary>
/// Automations for laundry.
/// </summary>
[NetDaemonApp]
public class Laundry
{
    private readonly IServices services;
    private readonly ILogger<Laundry> logger;
    
    /// <summary>
    /// Sets up laundry automations.
    /// </summary>
    public Laundry(IHaContext context, ILogger<Laundry> logger)
    {
        var entities = new Entities(context);
        services = new Services(context);
        this.logger = logger;

        entities.Sensor.WasherWasherMachineState
            .StateChanges()
            .Where(x =>
                x.Old?.State == "run" &&
                x.New?.State == "stop")
            .Subscribe(_ => NotifyFamily(true));
        
        // Usually, state becomes "finished", but occasionally goes from "cooling" to "none".
        entities.Sensor.DryerDryerMachineState
            .StateChanges()
            .Where(x =>
                x.Old?.State == "run" &&
                x.New?.State == "stop")
            .Subscribe(_ => NotifyFamily(false));
    }

    /// <summary>
    /// Notifies users that the washer or dryer has completed.
    /// </summary>
    private void NotifyFamily(bool washer)
    {
        logger.LogInformation("Notifying family that the {Device} has completed.", washer ? "washer" : "dryer");

        var device = washer ? "washer" : "dryer";
        services.Notify.Family(
            $"The {device} has completed!",
            "Laundry");
    }
}