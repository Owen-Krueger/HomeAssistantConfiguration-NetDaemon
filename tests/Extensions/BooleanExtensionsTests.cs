using NetDaemon.Extensions;

namespace NetDaemon.Tests.Extensions;

public class BooleanExtensionsTests
{
    [TestCase(true, "On")]
    [TestCase(false, "Off")]
    public void GetOnOffString_BooleanInput_ExpectedResult(bool input, string expectedResult)
    {
        var result = input.GetOnOffString();
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCaseSource(nameof(GetOnOffStringFromStateTestCases))]
    public void GetOnOffStringFromState_StateInput_ExpectedResult(string input, string expectedResult)
    {
        var result = input.GetOnOffStringFromState();
        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    private static IEnumerable<object> GetOnOffStringFromStateTestCases()
    {
        yield return new object[] { "true", "On" };
        yield return new object[] { "TrUe", "On" };
        yield return new object[] { "false", "Off" };
        yield return new object[] { "Unexpected", "Unknown" };
        yield return new object?[] { null, "Unknown" };
    }
}