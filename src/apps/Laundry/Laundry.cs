using HomeAssistantGenerated;
using NetDaemon.Utilities;

namespace NetDaemon.apps.Laundry;

/// <summary>
/// Automations for laundry.
/// </summary>
[NetDaemonApp]
public class Laundry
{
    private readonly IServices services;
    
    /// <summary>
    /// Sets up laundry automations.
    /// </summary>
    public Laundry(IHaContext context)
    {
        var entities = new Entities(context);
        services = new Services(context);

        entities.Sensor.WasherWasherMachineState
            .StateChanges()
            .Where(x =>
                x.Old?.State == "run" &&
                x.New?.State == "stop")
            .Subscribe(_ => NotifyUsers(true));
        
        // Usually, state becomes "finished", but occasionally goes from "cooling" to "none".
        entities.Sensor.DryerDryerMachineState
            .StateChanges()
            .Where(x =>
                x.Old?.State == "run" &&
                x.New?.State == "stop")
            .Subscribe(_ => NotifyUsers(false));
    }

    /// <summary>
    /// Notifies users that the washer or dryer has completed.
    /// </summary>
    private void NotifyUsers(bool washer)
    {
        var device = washer ? "washer" : "dryer";
        services.Notify.NotifyAll(
            $"The {device} has completed!",
            "Laundry");
    }
}