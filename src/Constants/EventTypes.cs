namespace NetDaemon.Constants;

/// <summary>
/// Keys for various events that can be triggered.
/// </summary>
public static class EventTypes
{
    /// <summary>
    /// An action selected from a mobile app notification.
    /// </summary>
    public const string MobileAppNotificationActionEvent = "mobile_app_notification_action";

    /// <summary>
    /// An event from a zigbee device.
    /// </summary>
    public const string ZhaEvent = "zha_event";
}