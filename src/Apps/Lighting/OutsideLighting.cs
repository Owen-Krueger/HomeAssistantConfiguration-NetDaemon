using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Apps.Lighting;

/// <summary>
/// Automations for outside lighting.
/// </summary>
[NetDaemonApp]
public class OutsideLighting
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly IScheduler scheduler;
    private readonly ILogger<OutsideLighting> logger;
    
    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public OutsideLighting(IHaContext context, IScheduler scheduler, ILogger<OutsideLighting> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.logger = logger;
        this.scheduler = scheduler;

        // Schedule-based automations.
        SetUpSunsetTriggers();
        scheduler.ScheduleCron("0 22 * * *", TurnOffPorchTimeBased);
        scheduler.ScheduleCron("0 0 * * *", TurnOffPorch); // Turn off the porch at midnight, no matter what.

        // Location-based automations.
        entities.Sensor.AllisonDistanceMiles
            .StateChanges()
            .Where(x =>
                    x.Old?.State > 5 &&
                    x.New?.State <= 5)
            .Subscribe(_ => SetPorchLightingStateLate(true));
        entities.Sensor.OwenDistanceMiles
            .StateChanges()
            .Where(x =>
                x.Old?.State > 5 &&
                x.New?.State <= 5)
            .Subscribe(_ => SetPorchLightingStateLate(true));
        entities.Person.Allison
            .StateChanges()
            .WhenStateIsFor(x => x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => SetPorchLightingStateLate(false));
        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => SetPorchLightingStateLate(false));

        // State-based automations
        entities.BinarySensor.G4DoorbellProPersonDetected
            .StateChanges()
            .Where(x => x.New.IsOn())
            .Subscribe(_ => SetPorchLightingStateLate(true));
        entities.BinarySensor.G4DoorbellProPersonDetected
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromMinutes(2), scheduler)
            .Subscribe(_ => SetPorchLightingStateLate(false));
    }

    /// <summary>
    /// Sets up the triggers to turn on the porch and set up the trigger for the following day.
    /// Getting the trigger for the following day 1 minute after sunset should have the next
    /// sunset time exposed in HomeAssistant.
    /// </summary>
    private void SetUpSunsetTriggers()
    {
        var nextSunset = GetNextTimeFromSensor(entities.Sensor.SunNextSetting, DateTime.Today.AddHours(17));
        var nextTrigger = nextSunset.AddMinutes(-15);
        logger.LogInformation("Next time to turn on porch set to {Date}", nextTrigger.ToUsCentralTime());
        
        scheduler.Schedule(nextTrigger, () => SetPorchLightingState(true, true));
        scheduler.Schedule(nextSunset.AddMinutes(1), SetUpSunsetTriggers);
    }

    /// <summary>
    /// Turns on/off porch lights if it's late.
    /// </summary>
    private void SetPorchLightingStateLate(bool isOn)
    {
        var nextSunrise = GetNextTimeFromSensor(entities.Sensor.SunNextRising, DateTime.Today.AddHours(6));
        
        if (scheduler.Now.IsBetween(new TimeOnly(22, 0), new TimeOnly(nextSunrise.TimeOfDay.Ticks)))
        {
            SetPorchLightingState(isOn, false);
        }
    }

    /// <summary>
    /// Turns off the porch lights if someone is almost home.
    /// </summary>
    private void TurnOffPorchTimeBased()
    {
        var owenDistance = entities.Sensor.OwenDistanceMiles.State ?? 0;
        var allisonDistance = entities.Sensor.AllisonDistanceMiles.State ?? 0;

        // If someone is close to home, don't turn off the lights yet.
        if (allisonDistance is > 0 and < 5 && owenDistance is > 0 and < 5)
        {
            logger.LogInformation("Someone getting close to home. Not turning off front porch lights yet.");
            return;
        }
     
        SetPorchLightingState(false, true);
    }

    /// <summary>
    /// Turns off porch lights if they're actively on.
    /// </summary>
    private void TurnOffPorch()
        => SetPorchLightingState(false, true);

    /// <summary>
    /// Turns on or off the porch and holiday lights.
    /// </summary>
    /// <param name="isOn">Whether to turn on the lights.</param>
    /// <param name="turnOnHolidayLights">Whether to turn on holiday lights, if around a holiday.</param>
    private void SetPorchLightingState(bool isOn, bool turnOnHolidayLights)
    {
        logger.LogInformation("Turning the porch lights {State}.", isOn.GetOnOffString());
        switch (isOn)
        {
            case true when entities.Switch.FrontPorchLights.IsOff():
                entities.Switch.FrontPorchLights.TurnOn();
                break;
            case false when entities.Switch.FrontPorchLights.IsOn():
                entities.Switch.FrontPorchLights.TurnOff();
                break;
        }

        if (!turnOnHolidayLights || !entities.InputBoolean.HolidayMode.IsOn())
        {
            return;
        }

        logger.LogInformation("Turning holiday lights {State}", isOn.GetOnOffString());
        var holidayLightsTarget = ServiceTarget.FromEntity(entities.Group.HolidayLights.EntityId);
        switch (isOn)
        {
            case true when entities.Group.HolidayLights.IsOff():
                services.Switch.TurnOn(holidayLightsTarget);
                break;
            case false when entities.Group.HolidayLights.IsOn():
                services.Switch.TurnOff(holidayLightsTarget);
                break;
        }
    }

    /// <summary>
    /// Gets the time of the next sunset/sunrise (depending on the sensor).
    /// </summary>
    private DateTimeOffset GetNextTimeFromSensor(SensorEntity sensor, DateTimeOffset defaultTime)
    {
        if (DateTimeOffset.TryParse(sensor.EntityState?.State ?? string.Empty,
                out var date))
        {
            return date;
        }

        logger.LogWarning("Failed to get sunset time from state. Defaulting to {Time}.",
            defaultTime.ToUsCentralTime().TimeOfDay);
        return defaultTime;
    }
}