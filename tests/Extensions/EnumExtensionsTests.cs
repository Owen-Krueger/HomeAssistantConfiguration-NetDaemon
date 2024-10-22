using HomeAssistantGenerated;
using NetDaemon.Extensions;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Tests.Extensions;

public class EnumExtensionsTests
{
    private IFixture fixture;

    [SetUp]
    public void Initialize()
    {
        fixture = new Fixture().Customize(new AutoMoqCustomization());
    }

    [TestCaseSource(nameof(GetEnumFromStateTestCases))]
    public void GetEnumFromState_VariousInputs_ExpectedResults(string state, TestEnum expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var inputSelect = new InputSelectEntity(contextMock.Object, entityId);
        var result = inputSelect.GetEnumFromState<TestEnum>();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private static IEnumerable<object> GetEnumFromStateTestCases()
    {
        yield return new object[] { TestEnum.Value1.ToString(), TestEnum.Value1 };
        yield return new object[] { TestEnum.Value2.ToString(), TestEnum.Value2 };
        yield return new object[] { "UnexpectedValue", TestEnum.Value1 };
        yield return new object?[] { null, TestEnum.Value1 };
    }
}

public enum TestEnum
{
    Value1,

    Value2
}