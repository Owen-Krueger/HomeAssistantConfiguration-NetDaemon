using HomeAssistantGenerated;
using NetDaemon.Extensions;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Models.Enums;

namespace NetDaemon.Tests.Extensions;

public class PersonExtensionsTests
{
    private IFixture fixture;

    [SetUp]
    public void Initialize()
    {
        fixture = new Fixture().Customize(new AutoMoqCustomization());
    }
    
    [TestCase("home", true)]
    [TestCase("away", false)]
    [TestCase("something", false)]
    public void IsHome_PersonEntityVariousStates_ExpectedResult(string state, bool expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var person = new PersonEntity(contextMock.Object, entityId);
        var result = person.IsHome();
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [TestCase("home", true)]
    [TestCase("away", false)]
    public void IsHome_PersonAttributesVariousStates_ExpectedResult(string state, bool expectedResult)
    {
        var entityState = new EntityState { State = state };
        var personEntityState = new EntityState<PersonAttributes>(entityState);
        var result = personEntityState.IsHome();
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}