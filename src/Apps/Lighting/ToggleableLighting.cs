using System.Collections.Generic;
using System.Linq;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Lighting;

/// <summary>
/// Automations for lights that can be toggled on and off.
/// </summary>
[NetDaemonApp]
public class ToggleableLighting
{
    private readonly ILogger<ToggleableLighting> logger;
    
    /// <summary>
    /// Sets up automations.
    /// </summary>
    public ToggleableLighting(IHaContext context, ILogger<ToggleableLighting> logger)
    {
        var entities = new Entities(context);
        this.logger = logger;

        List<ToggleableLightingGroup> entityGroups =
        [
            new ToggleableLightingGroup
            {
                EventDeviceId = DeviceIds.DiningRoomButtonId,
                EventCommand = "single",
                Lights = [entities.Switch.DiningRoomLights]
            },
            new ToggleableLightingGroup
            {
                EventDeviceId = DeviceIds.AllisonLivingRoomButtonId,
                EventCommand = "on",
                Lights = [entities.Switch.AllisonLivingRoomLamp, entities.Switch.OwenLivingRoomLamp]
            },
            new ToggleableLightingGroup
            {
                EventDeviceId = DeviceIds.OwenLivingRoomButtonId,
                EventCommand = "on",
                Lights = [entities.Switch.AllisonLivingRoomLamp, entities.Switch.OwenLivingRoomLamp]
            }
        ];

        foreach (var group in entityGroups)
        {
            context.Events.Filter<ZhaEvent>(EventTypes.ZhaEvent)
                .Where(x => 
                    x.Data?.DeviceId == group.EventDeviceId &&
                    x.Data?.Command == group.EventCommand)
                .Subscribe(_ => ToggleLights(group));
        }
    }

    /// <summary>
    /// Toggles the lights in the provided group, as long as they haven't been toggled recently.
    /// </summary>
    private void ToggleLights(ToggleableLightingGroup group)
    {
        if (group.Lights.Count == 0)
        {
            return; // Can't toggle lights if there are no lights to toggle.
        }
        
        var devicesString = group.Lights.Select(x => x.EntityId);
        var recentlyTriggered = group.Lights.Exists(WasRecentlyTriggered);
        if (recentlyTriggered)
        {
            logger.LogInformation("Toggle requested for {Lights}, but they've recently been triggered. Skipping.",
                devicesString);
            return;
        }

        // Work-around from lights that get out of sync.
        var firstLightState = group.Lights[0].EntityState?.IsOn() ?? false; 
        logger.LogInformation("Toggle requested for {Lights}. Turning lights {State}.",
            devicesString, (!firstLightState).GetOnOffString());

        foreach (var light in group.Lights)
        {
            if (firstLightState)
            {
                light.TurnOff();
                return;
            }

            light.TurnOn();
        }
    }

    /// <summary>
    /// Returns if the entity has been triggered in the last two seconds.
    /// </summary>
    private static bool WasRecentlyTriggered(SwitchEntity entity)
        => entity.EntityState?.LastChanged > DateTime.Now.AddSeconds(-2);
}

/// <summary>
/// A group of lights that can be toggled by a button press event.
/// </summary>
internal record ToggleableLightingGroup
{
    /// <summary>
    /// The device that, when pressed, triggers the toggle.
    /// </summary>
    public string EventDeviceId { get; init; } = string.Empty;

    /// <summary>
    /// The event command to filter for that triggers the toggle.
    /// </summary>
    public string EventCommand { get; init; } = string.Empty;

    /// <summary>
    /// The devices that are toggled when the event is consumed.
    /// </summary>
    public List<SwitchEntity> Lights { get; init; } = [];
}