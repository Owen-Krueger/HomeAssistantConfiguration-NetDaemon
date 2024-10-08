﻿using System.Text.Json.Serialization;

namespace NetDaemon.Events;

/// <summary>
/// Event data from a button press.
/// </summary>
public record ZhaEvent
{
    /// <summary>
    /// Instantiates an empty <see cref="ZhaEvent"/> record.
    /// </summary>
    public ZhaEvent() { }
    
    [JsonPropertyName("device_id")] public string? DeviceId { get; init; }
    [JsonPropertyName("command")] public string? Command { get; init; }
}