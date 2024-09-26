using NetDaemon.Models;

namespace NetDaemon.Events;

public record MobileNotificationActionEvent
{
    /// <summary>
    /// Instantiates an empty <see cref="MobileAppNotificationAction"/> record.
    /// </summary>
    public MobileNotificationActionEvent() { }

    public MobileAppNotificationAction Action { get; set; } = new();
}