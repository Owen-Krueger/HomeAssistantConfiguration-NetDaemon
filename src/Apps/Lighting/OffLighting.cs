using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations dedicated to conditions where all lights should turn off.
/// </summary>
[NetDaemonApp]
public class OffLighting
{
    private readonly IEntities entities;
    private readonly ILogger<OffLighting> logger;

    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public OffLighting(IHaContext context, IScheduler scheduler, ILogger<OffLighting> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "not_home", TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOffAllLights());
        entities.Person.Allison
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "not_home", TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => TurnOffAllLights());
        entities.Sensor.OwenPhoneChargerType
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "wireless", TimeSpan.FromSeconds(10), scheduler)
            .Subscribe(_ => TurnOffAllLightsAtNight());
        context.Events.Where(x => x.EventType == "CUSTOM_EVENT_NIGHT_LIGHTING")
            .Subscribe(_ => ActivateNightLighting());
    }

    /// <summary>
    /// Turns off all lights based on if anyone is home.
    /// </summary>
    private void TurnOffAllLights()
    {
        if (entities.InputBoolean.ModeGuest.IsOn())
        {
            logger.LogInformation("Not turning off lights due to house being in guest mode.");
            return;
        }

        if (!entities.IsAnyoneHome())
        {
            logger.LogInformation("Everyone away. Turning off all lights.");
            entities.Scene.AllOff.TurnOn();
        }
        else
        {
            logger.LogInformation("Turning off lights depending on states.");
            entities.Scene.AllOffDynamic.TurnOn();
            TurnOffLightsBasedOnState();
        }
    }

    /// <summary>
    /// Turns off all lights at night.
    /// </summary>
    private void TurnOffAllLightsAtNight()
    {
        if (entities.InputBoolean.ModeVacation.IsOn() ||
            !entities.Person.Owen.IsHome() ||
            !DateTimeOffset.Now.IsBetween(new TimeOnly(20, 30), new TimeOnly(03, 0)))
        {
            return;
        }
        
        logger.LogInformation("Turning off all lights due to phone charging at night.");
        TurnOffAllLights();
    }

    /// <summary>
    /// Activates the "night lighting" scene.
    /// </summary>
    private void ActivateNightLighting()
    {
        logger.LogInformation("Activating night lighting.");
        entities.Scene.NightLighting.TurnOn();
    }

    /// <summary>
    /// Turns off lights in various parts of the house, depending on if they're actively being used or not.
    /// </summary>
    private void TurnOffLightsBasedOnState()
    {
        if (entities.Group.UpstairsActive.IsOff())
        {
            logger.LogInformation("Turning off upstairs living area.");
            entities.Scene.UpstairsLivingAreaOff.TurnOn();
        }

        if (entities.Group.DownstairsActive.IsOff())
        {
            logger.LogInformation("Turning off downstairs lights.");
            entities.Light.DownstairsLights.TurnOff();
        }

        if (entities.BinarySensor.OwenComputerActive.IsOff())
        {
            logger.LogInformation("Turning off office lights.");
            entities.Switch.OfficeLights.TurnOff();
        }
    }
}