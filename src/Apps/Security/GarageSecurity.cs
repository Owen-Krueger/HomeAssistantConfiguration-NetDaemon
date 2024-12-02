using System.Reactive.Concurrency;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models;
using NetDaemon.Models.Enums;
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
    private readonly IScheduler scheduler;
    private readonly ILogger<GarageSecurity> logger;
    private DateTimeOffset lastExecution;
    private const string CloseGarageDoorAction = "CLOSE_GARAGE_DOOR";

    /// <summary>
    /// Sets up the automations.
    /// </summary>
    public GarageSecurity(IHaContext context, IScheduler scheduler, ILogger<GarageSecurity> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.scheduler = scheduler;
        this.logger = logger;
        lastExecution = scheduler.Now.AddDays(-1);

        entities.InputSelect.HomeState
            .StateChanges()
            .Where(x => x.New.GetEnumFromState<HomeStateEnum>() == HomeStateEnum.Away)
            .Subscribe(_ => SendGarageDoorOpenNotification());
        context.Events.Filter<MobileNotificationActionEvent>(EventTypes.MobileAppNotificationActionEvent)
            .Where(x => x.Data?.Action == CloseGarageDoorAction)
            .Subscribe(_ => OpenShutGarageDoor(false));
    }

    /// <summary>
    /// Sends a notification if nobody is home and the garage door is open.
    /// </summary>
    private void SendGarageDoorOpenNotification()
    {
        if (entities.IsAnyoneHome() || entities.Cover.PrimaryGarageDoor.State == "closed" || 
            lastExecution >= scheduler.Now.AddMinutes(-5)) // To prevent two notifications if Owen and Allison are traveling together.
        {
            return;
        }
        
        lastExecution = scheduler.Now;
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
    /// Opens or shuts the garage door.
    /// </summary>
    private void OpenShutGarageDoor(bool openDoor)
    {
        if (openDoor)
        {
            logger.LogInformation("Opening garage door.");
            entities.Cover.PrimaryGarageDoor.OpenCover();
            return;
        }
        
        logger.LogInformation("Closing garage door.");
        entities.Cover.PrimaryGarageDoor.CloseCover();
    }
}