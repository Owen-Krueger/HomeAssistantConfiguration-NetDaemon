using System.Collections.Generic;
using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Lighting;

/// <summary>
/// Automations for lighting while on vacation.
/// </summary>
[NetDaemonApp]
public class VacationLighting
{
    private readonly IEntities entities;
    private readonly IScheduler scheduler;
    private readonly ILogger<VacationLighting> logger;
    private readonly List<IDisposable> automationTriggers = [];

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public VacationLighting(IHaContext context, IScheduler scheduler, ILogger<VacationLighting> logger)
    {
        entities = new Entities(context);
        this.scheduler = scheduler;
        this.logger = logger;
        TriggerUtilities.UpdateAutomationTriggers(automationTriggers,
            entities.InputBoolean.ModeVacation.IsOn(),
            SetupAutomationTriggers);

        entities.InputBoolean.ModeVacation
            .StateChanges()
            .Subscribe(x => 
                TriggerUtilities.UpdateAutomationTriggers(automationTriggers,
                    x.New.IsOn(), SetupAutomationTriggers));
    }

    /// <summary>
    /// Sets up all automation triggers.
    /// </summary>
    private List<IDisposable> SetupAutomationTriggers() =>
        [
            scheduler.ScheduleCron("1 16 * * *", () => SetKitchenLightsState(true)), // 4:01 PM
            scheduler.ScheduleCron("20 18 * * *", () => SetLightState(entities.Light.DownstairsLights, true)), // 6:20 PM
            scheduler.ScheduleCron("21 18 * * *", () => SetKitchenLightsState(true)), // 6:21 PM
            scheduler.ScheduleCron("6 21 * * *", () => SetLightState(entities.Light.BedroomLamps, true)), // 9:06 PM
            scheduler.ScheduleCron("10 21 * * *", () => SetLightState(entities.Light.DownstairsLights, true)), // 9:10 PM
            scheduler.ScheduleCron("3 23 * * *", () => SetLightState(entities.Light.BedroomLamps, false)) // 11:03 PM
        ];
    
    /// <summary>
    /// Turns on/off kitchen light switch.
    /// </summary>
    private void SetKitchenLightsState(bool isOn)
    {
        var lightsOn = entities.Switch.KitchenLights.IsOn();
        switch (isOn)
        {
            case true when !lightsOn: 
                logger.LogInformation("Turning on {Entity}", entities.Switch.KitchenLights.EntityId);
                entities.Switch.KitchenLights.TurnOn();
                break;
            case false when lightsOn:
                logger.LogInformation("Turning off {Entity}", entities.Switch.KitchenLights.EntityId);
                entities.Switch.KitchenLights.TurnOff();
                break;
        }
    }

    /// <summary>
    /// Turns on/off the <see cref="entity"/>.
    /// </summary>
    private void SetLightState(LightEntity entity, bool isOn)
    {
        var lightsOn = entity.IsOn();
        switch (isOn)
        {
            case true when !lightsOn:
                logger.LogInformation("Turning on {Entity}", entity.EntityId);
                entity.TurnOn();
                break;
            case false when lightsOn:
                logger.LogInformation("Turning off {Entity}", entity.EntityId);
                entity.TurnOff();
                break;
        }
    }
}