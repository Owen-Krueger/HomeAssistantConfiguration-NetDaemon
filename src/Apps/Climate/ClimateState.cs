using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Apps.Climate;

/// <summary>
/// Automations to set <see cref="ThermostatState"/>.
/// </summary>
[NetDaemonApp]
public class ClimateState
{
    private readonly IEntities entities;
    private readonly IServices services;
    private readonly ILogger<ClimateState> logger;

    /// <summary>
    /// Sets up automations.
    /// </summary>
    public ClimateState(IHaContext context, IScheduler scheduler, ILogger<ClimateState> logger)
    {
        entities = new Entities(context);
        services = new Services(context);
        this.logger = logger;

        // entities.Person.Allison
        //     .StateChanges()
        //     .Where(x => x.New.IsHome())
        //     .Subscribe(_ => SetHomeState());
        // entities.Person.Owen
        //     .StateChanges()
        //     .Where(x => x.New.IsHome())
        //     .Subscribe(_ => SetHomeState());
        // entities.Person.Allison
        //     .StateChanges()
        //     .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(15), scheduler)
        //     .Subscribe(_ => SetAwayState());
        // entities.Person.Owen
        //     .StateChanges()
        //     .WhenStateIsFor(x => !x.IsHome(), TimeSpan.FromMinutes(15), scheduler)
        //     .Subscribe(_ => SetAwayState());
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
        
        SetState(ThermostatState.Home);
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
        
        SetState(ThermostatState.Away);
    }

    /// <summary>
    /// Sets the state of the house to the input.
    /// </summary>
    private void SetState(ThermostatState state)
    {
        if (GetThermostatState() == state)
        {
            return;
        }
        
        logger.LogInformation("Setting thermostat state to {State}", state);
        
        var target = ServiceTarget.FromEntity(entities.InputSelect.ThermostatState.EntityId);
        services.InputSelect.SelectOption(target, state.ToString());
    }

    /// <summary>
    /// Gets the current state of the house as <see cref="ThermostatState"/>.
    /// </summary>
    private ThermostatState GetThermostatState()
        => entities.InputSelect.ThermostatState.GetEnumFromState<ThermostatState>();
}