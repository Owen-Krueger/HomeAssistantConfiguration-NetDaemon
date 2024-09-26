using NetDaemon.Constants;
using NetDaemon.Events;
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
    private const string CloseGarageDoorAction = "CLOSE_GARAGE_DOOR";

    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public GarageSecurity(IHaContext context, ILogger<GarageSecurity> logger)
    {
        entities = new Entities(context);
        this.logger = logger;

        context.Events.Filter<MobileNotificationActionEvent>(EventTypes.MobileAppNotificationActionEvent)
            .Where(x => x.Data?.Action == CloseGarageDoorAction)
            .Subscribe(x =>
            {
                logger.LogInformation("Request that garage door close.");
            });

        var services = new Services(context);
        services.Notify.Owen("Garage door is open.", "Garage", 
            data: new MobileAppNotificationData
        {
            Actions = [
                new MobileAppNotificationAction
                {
                    Action = CloseGarageDoorAction,
                    Title = "Close Garage Door",
                    Uri = "/dashboard-mobile-plus/0"
                }
            ]
        });
    }
}