using NetDaemon.Models;

namespace NetDaemon.Events;

public class MobileNotificationActionEvent
{
    public MobileAppNotificationAction Action { get; set; }
}