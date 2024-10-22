using System.Collections.Generic;
using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Lighting;

/// <summary>
/// Automations for inside holiday lighting.
/// </summary>
[NetDaemonApp]
public class HolidayLighting
{
    private readonly IEntities entities;
    private readonly IScheduler scheduler;
    private readonly ILogger<HolidayLighting> logger;
    private readonly List<IDisposable> automationTriggers = [];

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public HolidayLighting(IHaContext context, IScheduler scheduler, ILogger<HolidayLighting> logger)
    {
        entities = new Entities(context);
        this.scheduler = scheduler;
        this.logger = logger;
        TriggerUtilities.UpdateAutomationTriggers(automationTriggers,
            entities.InputBoolean.HolidayMode.IsOn(),
            SetupAutomationTriggers);

        entities.InputBoolean.HolidayMode
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
        scheduler.ScheduleCron("0 7 * * *", TurnOnHolidayLights),
        scheduler.ScheduleCron("0 22 * * *", TurnOffHolidayLights),
        entities.Person.Allison
            .StateChanges()
            .Where(x => x.New.IsHome())
            .Subscribe(_ => TurnOnHolidayLights()),
        entities.Person.Allison
            .StateChanges()
            .Where(x => !x.New.IsHome())
            .Subscribe(_ => TurnOffHolidayLights())
    ];

    /// <summary>
    /// Turns on the holiday lights if they're actively off.
    /// </summary>
    private void TurnOnHolidayLights()
        => SetHolidayLightsState(true);

    /// <summary>
    /// Turns off the holiday lights if they're actively on.
    /// </summary>
    private void TurnOffHolidayLights()
        => SetHolidayLightsState(false);
    
    /// <summary>
    /// Turns on or off the holiday lights based on the input.
    /// </summary>
    private void SetHolidayLightsState(bool isOn)
    {
        var christmasTreeSwitch = entities.Switch.ChristmasTreeSmartPlug;
        switch (isOn)
        {
            case true when christmasTreeSwitch.IsOff():
                logger.LogInformation("Turning on indoor holiday lights.");
                christmasTreeSwitch.TurnOn();
                break;
            case false when christmasTreeSwitch.IsOn():
                logger.LogInformation("Turning off indoor holiday lights.");
                christmasTreeSwitch.TurnOff();
                break;
        }
    }
}