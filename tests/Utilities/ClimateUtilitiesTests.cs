using HomeAssistantGenerated;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Utilities;

namespace NetDaemon.Tests.Utilities;

public class ClimateUtilitiesTests
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
        yield return new object?[] { null, false };
    }

    [Test]
    public void GetTimingThresholds_HeatMode_ExpectedThresholds()
    {
        var thresholds = ClimateUtilities.GetTimingThresholds(70, true);
        Assert.Multiple(() =>
        {
            Assert.That(thresholds[0].Temperature, Is.EqualTo(70));
            Assert.That(thresholds[0].MinutesToDesired, Is.EqualTo(0));
            Assert.That(thresholds[1].Temperature, Is.EqualTo(69));
            Assert.That(thresholds[1].MinutesToDesired, Is.EqualTo(10));
            Assert.That(thresholds[2].Temperature, Is.EqualTo(68));
            Assert.That(thresholds[2].MinutesToDesired, Is.EqualTo(20));
            Assert.That(thresholds[3].Temperature, Is.EqualTo(67));
            Assert.That(thresholds[3].MinutesToDesired, Is.EqualTo(30));
            Assert.That(thresholds[4].Temperature, Is.EqualTo(66));
            Assert.That(thresholds[4].MinutesToDesired, Is.EqualTo(40));
            Assert.That(thresholds[5].Temperature, Is.EqualTo(65));
            Assert.That(thresholds[5].MinutesToDesired, Is.EqualTo(50));
            Assert.That(thresholds[6].Temperature, Is.EqualTo(64));
            Assert.That(thresholds[6].MinutesToDesired, Is.EqualTo(60));
            Assert.That(thresholds[7].Temperature, Is.EqualTo(63));
            Assert.That(thresholds[7].MinutesToDesired, Is.EqualTo(70));
            Assert.That(thresholds[8].Temperature, Is.EqualTo(62));
            Assert.That(thresholds[8].MinutesToDesired, Is.EqualTo(80));
        });
    }
    
    [Test]
    public void GetTimingThresholds_CoolMode_ExpectedThresholds()
    {
        var thresholds = ClimateUtilities.GetTimingThresholds(70, false);
        Assert.Multiple(() =>
        {
            Assert.That(thresholds[0].Temperature, Is.EqualTo(70));
            Assert.That(thresholds[0].MinutesToDesired, Is.EqualTo(0));
            Assert.That(thresholds[1].Temperature, Is.EqualTo(71));
            Assert.That(thresholds[1].MinutesToDesired, Is.EqualTo(10));
            Assert.That(thresholds[2].Temperature, Is.EqualTo(72));
            Assert.That(thresholds[2].MinutesToDesired, Is.EqualTo(20));
            Assert.That(thresholds[3].Temperature, Is.EqualTo(73));
            Assert.That(thresholds[3].MinutesToDesired, Is.EqualTo(30));
            Assert.That(thresholds[4].Temperature, Is.EqualTo(74));
            Assert.That(thresholds[4].MinutesToDesired, Is.EqualTo(40));
            Assert.That(thresholds[5].Temperature, Is.EqualTo(75));
            Assert.That(thresholds[5].MinutesToDesired, Is.EqualTo(50));
            Assert.That(thresholds[6].Temperature, Is.EqualTo(76));
            Assert.That(thresholds[6].MinutesToDesired, Is.EqualTo(60));
            Assert.That(thresholds[7].Temperature, Is.EqualTo(77));
            Assert.That(thresholds[7].MinutesToDesired, Is.EqualTo(70));
            Assert.That(thresholds[8].Temperature, Is.EqualTo(78));
            Assert.That(thresholds[8].MinutesToDesired, Is.EqualTo(80));
        });
    }
}