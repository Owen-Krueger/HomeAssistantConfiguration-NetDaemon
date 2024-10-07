using System.Collections.Generic;
using System.Reactive.Concurrency;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations for inside holiday lighting.
/// </summary>
[NetDaemonApp]
public class HolidayLighting
{
    private readonly IEntities entities;
    private readonly IScheduler scheduler;
    private readonly ILogger<HolidayLighting> logger;
    private List<IDisposable> automationTriggers = [];

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public HolidayLighting(IHaContext context, IScheduler scheduler, ILogger<HolidayLighting> logger)
    {
        entities = new Entities(context);
        this.scheduler = scheduler;
        this.logger = logger;
        UpdateAutomationTriggers();

        entities.InputBoolean.HolidayMode
            .StateChanges()
            .Subscribe(_ => UpdateAutomationTriggers());
    }
    
    /// <summary>
    /// Updates the automation triggers. If holiday mode is on, sets up triggers if they're not actively on.
    /// If holiday mode is off, removes any triggers that are actively on.
    /// </summary>
    private void UpdateAutomationTriggers()
    {
        switch (entities.InputBoolean.HolidayMode.IsOn())
        {
            // Set up automation triggers
            case true when automationTriggers.Count == 0:
                automationTriggers.Add(scheduler.ScheduleCron("0 7 * * *", TurnOnHolidayLights));
                automationTriggers.Add(scheduler.ScheduleCron("0 22 * * *", TurnOffHolidayLights));
                automationTriggers.Add(entities.Person.Allison
                    .StateChanges()
                    .Where(x => x.New.IsHome())
                    .Subscribe(_ => TurnOnHolidayLights()));
                automationTriggers.Add(entities.Person.Allison
                    .StateChanges()
                    .Where(x => !x.New.IsHome())
                    .Subscribe(_ => TurnOffHolidayLights()));
                break;
            // Remove any existing automation triggers and turn off holiday lights if they're on.
            case false when automationTriggers.Count > 0:
                automationTriggers = automationTriggers.DisposeTriggers();
                TurnOffHolidayLights(); // Turn off the lights if they're on when we turn off holiday mode.
                break;
        }
    }

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