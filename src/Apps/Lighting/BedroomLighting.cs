using System.Text.Json.Serialization;
using HomeAssistantGenerated.Utilities;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.apps.Lighting;

[NetDaemonApp]
public class BedroomLighting
{
    private readonly IEntities entities;
    
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
                x.Old?.State == "off" && 
                x.New?.State == "on")
            .Subscribe(_ => ActivateNightLighting());
        entities.Switch.BedroomLights
            .StateChanges()
            .Where(x => 
                x.Old?.State == "on" && 
                x.New?.State == "off")
            .Subscribe(_ => ActivateNightLighting());
    }

    private void OnBedsideButtonPressed()
    {
        entities.Light.BedroomLamps.Toggle();

        if (IsLate() && entities.Switch.BedroomLights.IsOn())
        {
            entities.Switch.BedroomLights.TurnOff();
        }
    }

    private void ActivateNightLighting()
    {
        if (!IsLate())
        {
            return;
        }

        entities.Light.BedroomLamps.TurnOn();
        entities.Switch.BedroomLights.TurnOff();
    }

    private bool IsLate()
    {
        var currentTime = DateTimeOffset.Now.ToUsCentralTime().TimeOfDay;
        // Between 9PM and midnight.
        return currentTime > TimeSpan.FromHours(21) && currentTime < TimeSpan.FromMinutes(1439);
    }
}

internal record ZhaEventData
{
    [JsonPropertyName("device_id")] public string? DeviceId { get; init; }
    [JsonPropertyName("command")] public string? Command { get; init; }
}