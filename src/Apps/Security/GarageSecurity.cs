using System.Reactive.Concurrency;
using NetDaemon.Constants;
using NetDaemon.Events;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
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
    private readonly IScheduler scheduler;
    private readonly ILogger<GarageSecurity> logger;
    private DateTimeOffset lastExecution;
    private const string OpenGarageDoorAction = "OPEN_GARAGE_DOOR";
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

        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => SendGarageDoorOpenNotification());
        entities.Person.Allison
            .StateChanges()
            .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(5), scheduler)
            .Subscribe(_ => SendGarageDoorOpenNotification());
        context.Events.Filter<MobileNotificationActionEvent>(EventTypes.MobileAppNotificationActionEvent)
            .Where(x => x.Data?.Action == CloseGarageDoorAction)
            .Subscribe(_ => OpenShutGarageDoor(false));
        entities.InputBoolean.OwenOnMorningWalk
            .StateChanges()
            .Where(x => x.New.IsOn())
            .Subscribe(_ => SendOpenGarageDoorRequestNotification());
        context.Events.Filter<MobileNotificationActionEvent>(EventTypes.MobileAppNotificationActionEvent)
            .Where(x => x.Data?.Action == OpenGarageDoorAction)
            .Subscribe(_ => OpenShutGarageDoor(true));
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
    /// Sends notification to see if Owen wants the garage door open for bringing the garbage in.
    /// </summary>
    private void SendOpenGarageDoorRequestNotification()
    {
        if (scheduler.Now.DayOfWeek != DayOfWeek.Friday || entities.Cover.PrimaryGarageDoor.State == "open")
        {
            return;
        }
        
        services.Notify.Owen("Open Garage Door for Garbage?", "Garage", 
            data: new MobileAppNotificationData
            {
                Actions = [
                    new MobileAppNotificationAction
                    {
                        Action = OpenGarageDoorAction,
                        Title = "Open Garage Door",
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