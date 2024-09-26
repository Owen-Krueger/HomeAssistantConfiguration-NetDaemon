using NetDaemon.Utilities;

namespace NetDaemon.Tests.Utilities;

public class DateTimeOffsetExtensionsTests
{
    [TestCaseSource(nameof(IsBetweenTestCases))]
    public void IsBetween_VariousTestCases_ExpectedResult(DateTimeOffset date, TimeOnly startTime, TimeOnly endTime, bool expectedResult)
    {
        var result = date.IsBetween(startTime, endTime);
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    private static IEnumerable<object> IsBetweenTestCases()
    {
        var centralTimeZone = DateTimeOffsetExtensions.GetUsCentralTimeZoneInfo();
        var date = new DateTimeOffset(2024, 1, 1, 13, 0, 0, centralTimeZone.BaseUtcOffset);
        
        yield return new object[] { date, new TimeOnly(12, 59), new TimeOnly(13, 01), true };
        yield return new object[] { date, new TimeOnly(12, 00), new TimeOnly(12, 59), false };
        yield return new object[] { date, new TimeOnly(12, 59), new TimeOnly(10, 00), true };
        yield return new object[] { date, new TimeOnly(18, 00), new TimeOnly(13, 01), true };
    }

    [TestCaseSource(nameof(ConvertTimeTestCases))]
    public void ConvertTime_UsingTimezone_ConvertsProperly(DateTimeOffset startingDate, DateTimeOffset expectedConvertedDate)
    {
        var result = startingDate.ConvertTime(TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));

        Assert.That(result, Is.EqualTo(expectedConvertedDate));
    }
    
    [TestCaseSource(nameof(ConvertTimeTestCases))]
    public void ToUsCentralTime_UsingString_ConvertsProperly(DateTimeOffset startingDate, DateTimeOffset expectedConvertedDate)
    {
        var result = startingDate.ToUsCentralTime();

        Assert.That(result, Is.EqualTo(expectedConvertedDate));
    }

    private static IEnumerable<object> ConvertTimeTestCases()
    {
        yield return new object[]
        {
            new DateTimeOffset(2019, 11, 6, 17, 48, 25, TimeSpan.Zero),
            new DateTimeOffset(2019, 11, 6, 11, 48, 25, TimeSpan.FromHours(-6))
        };
        yield return new object[]
        {
            new DateTimeOffset(2019, 11, 1, 17, 48, 25, TimeSpan.Zero),
            new DateTimeOffset(2019, 11, 1, 12, 48, 25, TimeSpan.FromHours(-5))
        };
    }
}