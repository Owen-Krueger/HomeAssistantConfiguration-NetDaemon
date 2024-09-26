using System.Text.Json.Serialization;
using NetDaemon.Models;

namespace NetDaemon.Events;

/// <summary>
/// An event when an option is selected from a mobile app notification.
/// </summary>
public record MobileNotificationActionEvent
{
    /// <summary>
    /// Instantiates an empty <see cref="MobileAppNotificationAction"/> record.
    /// </summary>
    public MobileNotificationActionEvent() { }

    /// <summary>
    /// The key for the action to perform.
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; init; } = string.Empty;
    
    [JsonPropertyName("device_id")]
    public string? DeviceId { get; init; }
}