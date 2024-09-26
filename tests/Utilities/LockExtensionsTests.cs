using HomeAssistantGenerated;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Tests.Utilities;

public class LockExtensionsTests
{
    private IFixture fixture;

    [SetUp]
    public void Initialize()
    {
        fixture = new Fixture().Customize(new AutoMoqCustomization());
    }
    
    [TestCase("locked", true)]
    [TestCase("unlocked", false)]
    public void IsLocked_LockEntityVariousStates_ExpectedResult(string state, bool expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var lockEntity = new LockEntity(contextMock.Object, entityId);
        var result = lockEntity.IsLocked();
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [TestCase("locked", true)]
    [TestCase("unlocked", false)]
    public void IsHome_LockAttributesVariousStates_ExpectedResult(string state, bool expectedResult)
    {
        var entityState = new EntityState { State = state };
        var lockEntityState = new EntityState<LockAttributes>(entityState);
        var result = lockEntityState.IsLocked();
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}