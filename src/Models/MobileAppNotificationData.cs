﻿using System.Collections.Generic;

namespace NetDaemon.Models;

/// <summary>
/// Data included in a mobile app notification.
/// </summary>
public class MobileAppNotificationData
{
    /// <summary>
    /// Actions available on mobile notifications.
    /// </summary>
    public List<MobileAppNotificationAction> Actions { get; set; } = [];
}

/// <summary>
/// An action to perform from a mobile app notification.
/// </summary>
public class MobileAppNotificationAction
{
    /// <summary>
    /// Key of what action to perform.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Title of action for button to display.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Path to frontend, if clicked.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// IOS specific. Set to foreground to launch the app when tapped. Defaults to background which just fires
    /// the event.
    /// </summary>
    public string? ActivationMode { get; set; }

    /// <summary>
    /// IOS specific. True to require entering a passcode to use the action.
    /// </summary>
    public bool? AuthenticationRequired { get; set; }

    /// <summary>
    /// IOS specific. True to color the action's title red, indicating a destructive action.
    /// </summary>
    public bool? Destructive { get; set; }

    /// <summary>
    /// IOS specific. TextInput to prompt for text to return with the event. This also occurs when setting the
    /// action to REPLY.
    /// </summary>
    public string? Behavior { get; set; }

    /// <summary>
    /// IOS specific. Title to use for text input for actions that prompt.
    /// </summary>
    public string? TextInputButtonTitle { get; set; }

    /// <summary>
    /// IOS specific. Placeholder to use for text input for actions that prompt.
    /// </summary>
    public string? TextInputPlaceholder { get; set; }

    /// <summary>
    /// IOS specific. The icon to use for the notification.
    /// See: https://companion.home-assistant.io/docs/notifications/actionable-notifications#icon-values
    /// </summary>
    public string? Icon { get; set; }
}