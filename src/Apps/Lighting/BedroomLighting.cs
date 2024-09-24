using System.Text.Json.Serialization;
using HomeAssistantGenerated.Utilities;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.apps.Lighting;

/// <summary>
/// Automations for bedroom lighting.
/// </summary>
[NetDaemonApp]
public class BedroomLighting
{
    private readonly IEntities entities;
    
    /// <summary>
    /// Sets up automations.
    /// </summary>
    public BedroomLighting(IHaContext context)
    {
        entities = new Entities(context);

        context.Events.Filter<ZhaEventData>("zha_event")
            .Where(x =>
                x.Data is { DeviceId: "99d54b9a73f87bfe21094394baa6fecf", Command: "single" })
            .Subscribe(_ => OnBedsideButtonPressed());
        entities.Light.BedroomLamps
            .StateChanges()
            .Where(x => 
                x.Old.IsOff() && 
                x.New.IsOn())
            .Subscribe(_ => ActivateNightLighting());
        entities.Switch.BedroomLights
            .StateChanges()
            .Where(x => 
                x.Old.IsOn() && 
                x.New.IsOff())
            .Subscribe(_ => ActivateNightLighting());
    }

    /// <summary>
    /// When bedside button is pressed, toggle lamps. If it's nighttime and lamps are being turned on, turn
    /// off the bedroom lights.
    /// </summary>
    private void OnBedsideButtonPressed()
    {
        entities.Light.BedroomLamps.Toggle();

        if (IsLate() && entities.Switch.BedroomLights.IsOn())
        {
            entities.Switch.BedroomLights.TurnOff();
        }
    }

    /// <summary>
    /// If it's late, turn on lamps and turn off bedroom lights.
    /// </summary>
    private void ActivateNightLighting()
    {
        if (!IsLate())
        {
            return;
        }

        entities.Light.BedroomLamps.TurnOn();
        entities.Switch.BedroomLights.TurnOff();
    }

    /// <summary>
    /// Returns if it's between 9PM and midnight.
    /// </summary>
    private static bool IsLate()
    {
        var currentTime = DateTimeOffset.Now.ToUsCentralTime().TimeOfDay;
        return currentTime > TimeSpan.FromHours(21) && currentTime < TimeSpan.FromMinutes(1439);
    }
}

/// <summary>
/// Event data from a button press.
/// </summary>
internal record ZhaEventData
{
    [JsonPropertyName("device_id")] public string? DeviceId { get; init; }
    [JsonPropertyName("command")] public string? Command { get; init; }
}