using Microsoft.Reactive.Testing;
using NetDaemon.Utilities;

namespace NetDaemon.Tests.TestHelpers;

/// <summary>
/// Extensions for <see cref="TestScheduler"/>.
/// </summary>
public static class TestSchedulerExtensions
{
    /// <summary>
    /// Advances scheduler to now, according to <see cref="DateTimeOffset"/>.
    /// </summary>
    public static void AdvanceToNow(this TestScheduler testScheduler)
    {
        testScheduler.AdvanceTo(DateTimeOffset.Now.Ticks);
    }
    
    /// <summary>
    /// Advances <see cref="TestScheduler"/> to specified date and time. Provided <see cref="DateTime"/> expected to
    /// be in US Central timezone.
    /// </summary>
    public static void AdvanceTo(this TestScheduler testScheduler, DateTime dateTime)
    {
        var timeZone = DateTimeOffsetExtensions.GetUsCentralTimeZoneInfo();
        testScheduler.AdvanceBy(new DateTimeOffset(TimeZoneInfo.ConvertTime(dateTime, timeZone)).UtcDateTime.Ticks);
    }
}