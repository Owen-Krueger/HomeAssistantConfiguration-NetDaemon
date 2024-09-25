using NetDaemon.Models;

namespace NetDaemon.Apps.Security;

/// <summary>
/// Automations for ensuring the garage door is shut when we're gone.
/// </summary>
[NetDaemonApp]
public class GarageSecurity
{
    private readonly IEntities entities;
    private readonly ILogger<GarageSecurity> logger;

    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public GarageSecurity(IHaContext context, ILogger<GarageSecurity> logger)
    {
        entities = new Entities(context);
        this.logger = logger;
        
        context.Events.Where(e => e.EventType == "mobile_app_notification_action")
            .Subscribe(e => 
            {
                logger.LogInformation("Received event: {Event}", e.DataElement.ToString());
            });

        var services = new Services(context);
        services.Notify.Owen("Test", "Test", new MobileAppNotificationData
        {
            Actions = [
                new MobileAppNotificationAction
                {
                    Action = "TEST",
                    Title = "Title",
                    Uri = "/dashboard-mobile-plus/upstairs"
                }
            ]
        });
    }
}