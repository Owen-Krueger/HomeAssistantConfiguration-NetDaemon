using System.Reactive.Subjects;
using System.Text.Json;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Tests.TestHelpers;

public class HaContextMockImpl : IHaContext
{
    public Dictionary<string, EntityState> EntityStates { get; } = new();

    public Subject<StateChange> StateAllChangeSubject { get; } = new();

    public Subject<Event> EventsSubject { get; } = new();

    public IObservable<StateChange> StateAllChanges()
        => StateAllChangeSubject;

    public EntityState? GetState(string entityId)
        => EntityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities()
        => EntityStates.Keys.Select(x => new Entity(this, x)).ToList();

    public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        if (target?.EntityIds is null)
        {
            return;
        }

        var state = service switch
        {
            "turn_on" => "on",
            "turn_off" => "off",
            _ => null
        };

        if (state is null)
        {
            return;
        }

        foreach (var entityId in target.EntityIds)
        {
            TriggerStateChange(entityId, state);
        }
    }

    public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        throw new NotImplementedException();
    }

    public Area? GetAreaFromEntityId(string entityId)
    {
        throw new NotImplementedException();
    }

    public EntityRegistration? GetEntityRegistration(string entityId)
    {
        throw new NotImplementedException();
    }

    public void SendEvent(string eventType, object? data = null) { }

    public IObservable<Event> Events => EventsSubject;

    public void TriggerStateChange(string entityId, string stateValue, object? attributes = null)
    {
        var state = new EntityState { State = stateValue };
        if (attributes is not null)
        {
            state = state with { AttributesJson = attributes.AsJsonElement() };
        }
        
        TriggerStateChange(entityId, state);
    }

    public void TriggerStateChange(string entityId, EntityState newState)
    {
        var oldState = EntityStates.TryGetValue(entityId, out var currentState) ? currentState : null;
        EntityStates[entityId] = newState;
        StateAllChangeSubject.OnNext(new StateChange(new Entity(this, entityId), oldState, newState));
    }
}

public class HaContextMock : Mock<HaContextMockImpl>
{
    public HaContextMock()
    {
        CallBase = true;
    }
  
    public void TriggerStateChange(Entity entity, string newStateValue, object? attributes = null)
    {
        var newState = new EntityState { State = newStateValue };
        if (attributes != null)
        {
            newState = newState with { AttributesJson = attributes.AsJsonElement() };
        }

        TriggerStateChange(entity.EntityId, newState);
    }

    public void TriggerStateChange(string entityId, EntityState newState)
    {
        Object.TriggerStateChange(entityId, newState);
    }

    public void VerifyServiceCalled(Entity entity, string domain, string service, object? data = null) =>
        VerifyServiceCalled(entity, domain, service, data, Times.Once());
    
    public void VerifyServiceCalled(Entity entity, string domain, string service, object? data, Times times)
    {
        Verify(m => m.CallService(domain, service,
            It.Is<ServiceTarget?>(s => s!.EntityIds!.SingleOrDefault() == entity.EntityId),
            data), times);
    }

    public void TriggerEvent(Event @event)
    {
        Object.EventsSubject.OnNext(@event);
    }
}

public static class TestExtensions
{
    public static JsonElement AsJsonElement(this object value)
    {
        var jsonString = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }
}