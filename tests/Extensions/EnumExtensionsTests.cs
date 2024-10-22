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
    public void GetEnumFromState_EntityVariousInputs_ExpectedResults(string state, TestEnum expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var inputSelect = new InputSelectEntity(contextMock.Object, entityId);
        var result = inputSelect.GetEnumFromState<TestEnum>();

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [TestCaseSource(nameof(GetEnumFromStateTestCases))]
    public void GetEnumFromState_StateVariousInputs_ExpectedResults(string state, TestEnum expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var inputSelect = new InputSelectEntity(contextMock.Object, entityId);
        var result = inputSelect.EntityState.GetEnumFromState<TestEnum>();

        Assert.That(result, Is.EqualTo(expectedResult));
    }

    private static IEnumerable<object> GetEnumFromStateTestCases()
    {
        yield return new object[] { TestEnum.Value1.ToString(), TestEnum.Value1 };
        yield return new object[] { TestEnum.Value2.ToString(), TestEnum.Value2 };
        yield return new object[] { "UnexpectedValue", TestEnum.Value1 };
        yield return new object?[] { null, TestEnum.Value1 };
    }
    
    [TestCaseSource(nameof(GetEnumFromStateWithDefaultTestCases))]
    public void GetEnumFromState_EntityWithDefaultVariousInputs_ExpectedResults(string state, TestEnum defaultEnum, TestEnum expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var inputSelect = new InputSelectEntity(contextMock.Object, entityId);
        var result = inputSelect.GetEnumFromState(defaultEnum);

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [TestCaseSource(nameof(GetEnumFromStateWithDefaultTestCases))]
    public void GetEnumFromState_StateWithDefaultVariousInputs_ExpectedResults(string state, TestEnum defaultEnum, TestEnum expectedResult)
    {
        var mock = new AutoMocker();
        var entityId = fixture.Create<string>();
        var contextMock = mock.GetMock<IHaContext>();
        contextMock.Setup(x => x.GetState(entityId)).Returns(new EntityState { State = state });
        var inputSelect = new InputSelectEntity(contextMock.Object, entityId);
        var result = inputSelect.EntityState.GetEnumFromState(defaultEnum);

        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    private static IEnumerable<object> GetEnumFromStateWithDefaultTestCases()
    {
        yield return new object[] { TestEnum.Value1.ToString(), TestEnum.Value1, TestEnum.Value1 };
        yield return new object[] { TestEnum.Value2.ToString(), TestEnum.Value2, TestEnum.Value2 };
        yield return new object[] { "UnexpectedValue", TestEnum.Value2, TestEnum.Value2 };
        yield return new object?[] { null, TestEnum.Value2, TestEnum.Value2 };
    }

    [TestCase(TestEnum.Value1, "value1")]
    [TestCase(TestEnum.Value2, "value2")]
    public void ToStringLowerCase_VariousInputs_ExpectedResult(TestEnum value, string expectedResult)
    {
        Assert.That(value.ToStringLowerCase(), Is.EqualTo(expectedResult));
    }
}

public enum TestEnum
{
    Value1,

    Value2
}