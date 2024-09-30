using System.Reactive.Concurrency;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.Models;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Security;

/// <summary>
/// Automations for ensuring the garage door is shut when we're gone.
/// </summary>
[NetDaemonApp]
public class GarageSecurity
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly ILogger<GarageSecurity> logger;
    private DateTimeOffset lastExecution = DateTimeOffset.Now.AddDays(-1);
    private const string CloseGarageDoorAction = "CLOSE_GARAGE_DOOR";

    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public GarageSecurity(IHaContext context, IScheduler scheduler, ILogger<GarageSecurity> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.logger = logger;

        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => SendNotification());
        entities.Person.Allison
            .StateChanges()
            .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => SendNotification());
        context.Events.Filter<MobileNotificationActionEvent>(EventTypes.MobileAppNotificationActionEvent)
            .Where(x => x.Data?.Action == CloseGarageDoorAction)
            .Subscribe(_ => ShutGarageDoor());

    }

    /// <summary>
    /// Sends a notification if nobody is home and the garage door is open.
    /// </summary>
    private void SendNotification()
    {
        if (entities.IsAnyoneHome() || entities.Cover.PrimaryGarageDoor.State == "closed" || 
            lastExecution >= DateTimeOffset.Now.AddMinutes(-5)) // To prevent two notifications if Owen and Allison are traveling together.
        {
            return;
        }
        
        lastExecution = DateTimeOffset.Now;
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

    /// <summary>
    /// Shuts the garage door.
    /// </summary>
    private void ShutGarageDoor()
    {
        logger.LogInformation("Shutting garage door.");
        entities.Cover.PrimaryGarageDoor.CloseCover();
    }
}