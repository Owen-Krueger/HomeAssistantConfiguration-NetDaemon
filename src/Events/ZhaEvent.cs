using System.Text.Json.Serialization;

namespace NetDaemon.Events;

/// <summary>
/// Event data from a button press.
/// </summary>
public abstract record ZhaEvent
{
    [JsonPropertyName("device_id")] public string? DeviceId { get; init; }
    [JsonPropertyName("command")] public string? Command { get; init; }
}