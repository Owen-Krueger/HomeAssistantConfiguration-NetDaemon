using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations for lighting based on televisions.
/// </summary>
[NetDaemonApp]
public class TelevisionLighting
{
    private readonly IEntities entities;
    private readonly ILogger<TelevisionLighting> logger;

    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public TelevisionLighting(IHaContext context, IScheduler scheduler, ILogger<TelevisionLighting> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        entities.BinarySensor.UpstairsTvOn
            .StateChanges()
            .WhenStateIsFor(x => x.IsOn(), TimeSpan.FromSeconds(15), scheduler)
            .Subscribe(_ => TurnOnLights(true));
        entities.BinarySensor.DownstairsTvOn
            .StateChanges()
            .WhenStateIsFor(x => x.IsOn(), TimeSpan.FromSeconds(15), scheduler)
            .Subscribe(_ => TurnOnLights(false));
        entities.BinarySensor.UpstairsTvOn
            .StateChanges()
            .Where(x => x.New.IsOff())
            .Subscribe(_ => TurnOffLivingRoomLamps());
    }

    /// <summary>
    /// Turns on the upstairs living room lamps or downstairs lights if it's not too late and we're not on vacation.
    /// </summary>
    private void TurnOnLights(bool upstairs)
    {
        var entityString = upstairs ? "Upstairs TV" : "Downstairs TV";
        if (!DateTimeOffset.Now.IsBetween(new TimeOnly(5, 30), new TimeOnly(21, 0)))
        {
            logger.LogInformation("{Entity} is on, but it's late. Not turning on the lights.", entityString);
            return;
        }

        if (entities.InputBoolean.ModeVacation.IsOn())
        {
            logger.LogInformation("{Entity} is on, but in vacation mode. Not turning on the lights.", entityString);
            return;
        }

        switch (upstairs)
        {
            case true when entities.Group.LivingRoomLamps.IsOff():
                entities.Group.LivingRoomLamps.CallService("switch.turn_on");
                break;
            case false when entities.Light.DownstairsLights.IsOff():
                entities.Light.DownstairsLights.TurnOn();
                break;
        }
    }

    /// <summary>
    /// Turns off the upstairs living room lamps, if they're currently on.
    /// </summary>
    private void TurnOffLivingRoomLamps()
    {
        if (entities.Group.LivingRoomLamps.IsOff())
        {
            return;
        }
        
        entities.Group.LivingRoomLamps.CallService("switch.turn_off");
    }
}