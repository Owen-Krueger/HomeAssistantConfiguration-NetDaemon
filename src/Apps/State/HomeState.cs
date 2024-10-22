using System.Reactive.Concurrency;
using NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.State;

/// <summary>
/// Automations to set <see cref="HomeStateEnum"/>.
/// </summary>
[NetDaemonApp]
public class HomeState
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly ILogger<HomeState> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public HomeState(IHaContext context, IScheduler scheduler, ILogger<HomeState> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.logger = logger;

        entities.Person.Allison
            .StateChanges()
            .Where(x => x.New.IsHome())
            .Subscribe(_ => SetHomeState());
        entities.Person.Owen
            .StateChanges()
            .Where(x => x.New.IsHome())
            .Subscribe(_ => SetHomeState());
        entities.Person.Allison
            .StateChanges()
            .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(15), scheduler)
            .Subscribe(_ => SetAwayState());
        entities.Person.Owen
            .StateChanges()
            .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(15), scheduler)
            .Subscribe(_ => SetAwayState());
    }

    /// <summary>
    /// Sets state as "home", if anyone is home.
    /// </summary>
    private void SetHomeState()
    {
        if (!entities.IsAnyoneHome())
        {
            return;
        }
        
        SetState(HomeStateEnum.Home);
    }

    /// <summary>
    /// Sets state as "away", if nobody is home.
    /// </summary>
    private void SetAwayState()
    {
        if (entities.IsAnyoneHome())
        {
            return;
        }
        
        SetState(HomeStateEnum.Away);
    }

    /// <summary>
    /// Sets the state of the house to the input.
    /// </summary>
    private void SetState(HomeStateEnum stateEnum)
    {
        if (GetHomeState() == stateEnum)
        {
            return;
        }
        
        logger.LogInformation("Setting home state to {State}", stateEnum);
        
        var target = ServiceTarget.FromEntity(entities.InputSelect.HomeState.EntityId);
        services.InputSelect.SelectOption(target, stateEnum.ToString());
        NotifyStateUpdate(stateEnum);
    }
    
    /// <summary>
    /// Notifies Owen that the state has been updated (if he wants updates).
    /// </summary>
    private void NotifyStateUpdate(HomeStateEnum stateEnum)
    {
        if (entities.InputBoolean.ClimateNotifyLocationBased.IsOff())
        {
            return;
        }
        
        services.Notify.Owen($"Home state updated to {stateEnum}.", "Climate");
    }

    /// <summary>
    /// Gets the current state of the house as <see cref="HomeStateEnum"/>.
    /// </summary>
    private HomeStateEnum GetHomeState()
        => entities.InputSelect.HomeState.GetEnumFromState<HomeStateEnum>();
}