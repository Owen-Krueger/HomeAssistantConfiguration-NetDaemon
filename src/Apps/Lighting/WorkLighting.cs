using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Lighting;

/// <summary>
/// Automation for turning on lights around the house on days that Owen is working.
/// </summary>
[NetDaemonApp]
public class WorkLighting
{
    private readonly IEntities entities;
    private readonly ILogger<WorkLighting> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public WorkLighting(IHaContext context, ILogger<WorkLighting> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        entities.Switch.OfficeLights
            .StateChanges()
            .Where(x => x.New.IsOff())
            .Subscribe(_ => TurnOnDiningRoomLights());
    }

    /// <summary>
    /// Turns on dining room lights if it appears to be lunchtime on a workday.
    /// </summary>
    private void TurnOnDiningRoomLights()
    {
        if (entities.Switch.DiningRoomLights.IsOn() ||
            entities.InputBoolean.ModeGuest.IsOn() ||
            !entities.Person.Owen.IsHome() ||
            entities.BinarySensor.WorkdaySensor.IsOff() ||
            !DateTimeOffset.Now.IsBetween(new TimeOnly(11, 0), new TimeOnly(13, 30)))
        {
            return;
        }
        
        logger.LogInformation("Turning on dining room lights.");
        entities.Switch.DiningRoomLights.TurnOn();
    }
}