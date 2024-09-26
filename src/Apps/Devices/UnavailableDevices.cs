using System.Collections.Generic;
using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Apps.Devices;

/// <summary>
/// Automations for pinging unavailable entities to make them active.
/// </summary>
[NetDaemonApp]
public class UnavailableDevices
{
    private readonly IServices services;
    private readonly IScheduler scheduler;
    private readonly ILogger<UnavailableDevices> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public UnavailableDevices(IHaContext context, IScheduler scheduler, ILogger<UnavailableDevices> logger)
    {
        var entities = new Entities(context);
        services = new Services(context);
        this.scheduler = scheduler;
        this.logger = logger;

        List<EntityGroup> entityGroups =
        [
            new EntityGroup(entities.Switch.DownstairsTvSmartPlug),
            new EntityGroup(entities.Switch.UpstairsTvSmartPlug),
            new EntityGroup(entities.Switch.InternetModemSmartPlug),
            new EntityGroup(entities.Switch.OwenComputerSmartPlug),
            new EntityGroup(entities.Switch.AllisonLivingRoomLamp, entities.Button.AllisonLivingRoomLampPing, entities.Switch.OwenLivingRoomLamp),
            new EntityGroup(entities.Switch.OwenLivingRoomLamp, entities.Button.OwenLivingRoomLampPing, entities.Switch.AllisonLivingRoomLamp),
            new EntityGroup(entities.Switch.BedroomLights, entities.Button.BedroomLightsPing),
            new EntityGroup(entities.Switch.CounterLights, entities.Button.CounterLightsPing),
            new EntityGroup(entities.Switch.DiningRoomLights, entities.Button.DiningRoomLightsPing),
            new EntityGroup(entities.Switch.FrontPorchLights, entities.Button.FrontPorchLightsPing),
            new EntityGroup(entities.Switch.DeckStringLights, entities.Button.DeckStringLightsPing),
            new EntityGroup(entities.Switch.GarageLights, entities.Button.GarageLightsPing),
            new EntityGroup(entities.Switch.KitchenLights, entities.Button.KitchenLights),
            new EntityGroup(entities.Switch.LaundryRoomLights, entities.Button.LaundryRoomLightsPing),
            new EntityGroup(entities.Switch.OfficeLights, entities.Button.OfficeLightsPing),
            new EntityGroup(entities.Switch.StairwayLights, entities.Button.StairwayLightsPing),
            new EntityGroup(entities.Switch.UtilityRoomLights, entities.Button.UtilityRoomLightsPing),
        ];

        foreach (var group in entityGroups)
        {
            if (group.PingEntity is not null)
            {
                group.UnavailableEntity
                    .StateChanges()
                    .Where(x => x.New?.State == "unavailable")
                    .Subscribe(_ => PingEntity(group));
            }
            else
            {
                group.UnavailableEntity
                    .StateChanges()
                    .Where(x => x.New?.State == "unavailable")
                    .Subscribe(_ => NotifyEntityUnavailable(group.UnavailableEntity));
            }
        }
    }

    /// <summary>
    /// Notifies Owen that device is unavailable.
    /// </summary>
    private void NotifyEntityUnavailable(SwitchEntity entity)
    {
        logger.LogInformation("{Entity} is unavailable. Notifying Owen.", entity.EntityId);
        services.Notify.Owen($"{entity.EntityId} is unavailable", "Unavailable Device");
    }

    /// <summary>
    /// Pings the entity to try to bring it back.
    /// </summary>
    private void PingEntity(EntityGroup group, int count = 1)
    {
        logger.LogInformation("Pinging {Entity} because it's unavailable (Attempt {Count})", 
            group.UnavailableEntity.EntityId, count);
        
        services.Button.Press(ServiceTarget.FromEntity(group.PingEntity!.EntityId));
        group.PingEntity!.CallService("button/press");
        scheduler.Schedule(DateTimeOffset.Now.AddSeconds(10), 
            _ => VerifyEntityIsAlive(group, count));
    }

    /// <summary>
    /// Checks if the entity is back. If it's still unavailable, try again.
    /// </summary>
    private void VerifyEntityIsAlive(EntityGroup group, int count)
    {
        if (group.PingEntity!.State != "unavailable") // Ping fixed state.
        {
            SyncEntities(group.UnavailableEntity, group.SyncEntity);
            return;
        }

        if (count >= 3) // We were unsuccessful.
        {
            logger.LogWarning("{Entity} pinged multiple times but is still unavailable", 
                group.UnavailableEntity.EntityId);
            return;
        }

        // Try to ping again in 5 minutes.
        scheduler.Schedule(DateTimeOffset.Now.AddMinutes(5),
            _ => PingEntity(group, ++count));
    }

    /// <summary>
    /// Syncs the states between the two entities. 
    /// </summary>
    /// <param name="entityToUpdate">Entity to update to match state from <see cref="correctEntity"/></param>
    /// <param name="correctEntity">Entity to update <see cref="entityToUpdate"/> from.</param>
    private void SyncEntities(SwitchEntity entityToUpdate, SwitchEntity? correctEntity)
    {
        if (correctEntity is null) // No need to sync entities.
        {
            return;
        }

        var entityToUpdateState = entityToUpdate.IsOn();
        var correctEntityState = correctEntity.IsOn();

        if (entityToUpdateState == correctEntityState) // Entities already synced.
        {
            return;
        }

        if (correctEntityState)
        {
            services.Switch.TurnOn(ServiceTarget.FromEntity(entityToUpdate.EntityId));
            return;
        }

        entityToUpdate.TurnOff();
    }
}

/// <summary>
/// Group of entities to monitor.
/// </summary>
internal record EntityGroup
{
    /// <summary>
    /// Instantiates a new <see cref="EntityGroup"/> for a non-pingable entity.
    /// </summary>
    public EntityGroup(SwitchEntity unavailableEntity)
    {
        UnavailableEntity = unavailableEntity;
    }
    
    /// <summary>
    /// Instantiates a new <see cref="EntityGroup"/> for a pingable entity without an entity to sync to.
    /// </summary>
    public EntityGroup(SwitchEntity unavailableEntity, ButtonEntity pingEntity)
    {
        UnavailableEntity = unavailableEntity;
        PingEntity = pingEntity;
    }

    /// <summary>
    /// Instantiates a new <see cref="EntityGroup"/> for a pingable entity with an entity to sync to.
    /// </summary>
    public EntityGroup(SwitchEntity unavailableEntity, ButtonEntity pingEntity, SwitchEntity syncEntity)
    {
        UnavailableEntity = unavailableEntity;
        PingEntity = pingEntity;
        SyncEntity = syncEntity;
    }
    
    /// <summary>
    /// Entity to monitor for state becoming "unavailable".
    /// </summary>
    public SwitchEntity UnavailableEntity { get; set; }

    /// <summary>
    /// Optional. Button to press/ping to bring entity back from the dead.
    /// </summary>
    public ButtonEntity? PingEntity { get; set; }

    /// <summary>
    /// Optional. Represents the state the <see cref="UnavailableEntity"/> should be set to after becoming
    /// available again.
    /// </summary>
    public SwitchEntity? SyncEntity { get; set; }
}