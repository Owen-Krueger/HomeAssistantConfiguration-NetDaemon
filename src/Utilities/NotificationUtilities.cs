using HomeAssistantGenerated;

namespace NetDaemon.Utilities;

/// <summary>
/// Utilities for <see cref="NotifyServices"/>.
/// </summary>
public static class NotificationUtilities
{
    /// <summary>
    /// Notifies all users the same message.
    /// </summary>
    public static void NotifyAll(this NotifyServices service, string message, string? title = null,
        object? target = null, object? data = null)
    {
        service.Owen(message, title, target, data);
        service.Allison(message, title, target, data);
    }
}