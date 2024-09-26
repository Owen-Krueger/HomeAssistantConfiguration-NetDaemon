using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations for lighting in the garage.
/// </summary>
[NetDaemonApp]
public class GarageLighting
{
    private readonly IEntities entities;
    private readonly ILogger<GarageLighting> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public GarageLighting(IHaContext context, IScheduler scheduler, ILogger<GarageLighting> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        entities.Cover.PrimaryGarageDoor
            .StateChanges()
            .Where(x => x.New?.State is "opening" or "open")
            .Subscribe(_ => TurnOnLights());
        entities.BinarySensor.GarageLightsMotionDetection
            .StateChanges()
            .Where(x => x.New.IsOn())
            .Subscribe(_ => TurnOnLights());
        entities.Cover.PrimaryGarageDoor
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "closed", TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOffLights());
        entities.BinarySensor.GarageLightsMotionDetection
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOffLights());
    }

    /// <summary>
    /// Turns on garage lights.
    /// </summary>
    private void TurnOnLights()
    {
        if (!entities.Switch.GarageLights.IsOff())
        {
            return;
        }
        
        logger.LogInformation("Turning on garage lights.");
        entities.Switch.GarageLights.TurnOn();
    }

    /// <summary>
    /// Turns off the garage lights if there's no motion and the garage door isn't open.
    /// </summary>
    private void TurnOffLights()
    {
        if (entities.Switch.GarageLights.IsOff() || entities.BinarySensor.GarageLightsMotionDetection.IsOn() ||
            entities.Cover.PrimaryGarageDoor.State == "open")
        {
            return;
        }

        logger.LogInformation("Turning off garage lights.");
        entities.Switch.GarageLights.TurnOff();
    }
}