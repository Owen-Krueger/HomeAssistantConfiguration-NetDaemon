using HomeAssistantGenerated;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Tests.Utilities;

public class ClimateExtensionsTests
{
    private IFixture fixture;

    [SetUp]
    public void Initialize()
    {
        fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [TestCaseSource(nameof(IsHeatModeTestCases))]
    public void IsHeatMode_VariousInputs_ExpectedResults(string state, bool expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var inputSelect = new ClimateEntity(contextMock.Object, entityId);
        var result = inputSelect.IsHeatMode();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private static IEnumerable<object> IsHeatModeTestCases()
    {
        yield return new object[] { "heat", true };
        yield return new object[] { "cool", false };
        yield return new object[] { null, false };
    }
}