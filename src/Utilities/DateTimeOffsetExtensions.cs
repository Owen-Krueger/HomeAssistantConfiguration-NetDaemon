using System.Runtime.InteropServices;

namespace NetDaemon.Utilities;

/// <summary>
/// Extension methods for working with DateTimeOffsets.
/// </summary>
public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Returns if the provided <see cref="DateTimeOffset"/> is between the start and end times.
    /// </summary>
    public static bool IsBetween(this DateTimeOffset date, TimeOnly startTime, TimeOnly endTime)
    {
        var currentTimeOfDay = date.ToUsCentralTime().TimeOfDay;

        return currentTimeOfDay > startTime.ToTimeSpan() && currentTimeOfDay < endTime.ToTimeSpan();
    }
    
    /// <summary>
    /// Converts a time to the time in a particular time zone.
    /// </summary>
    /// <param name="dateTimeOffset">The date and time to convert.</param>
    /// <param name="destinationTimeZone">The time zone to convert  to.</param>
    /// <returns>The date and time in the destination time zone.</returns>
    public static DateTimeOffset ConvertTime(this DateTimeOffset dateTimeOffset, TimeZoneInfo destinationTimeZone)
    {
        return TimeZoneInfo.ConvertTime(dateTimeOffset, destinationTimeZone);
    }

    /// <summary>
    /// Converts a time to the time in the Central time zone.
    /// </summary>
    /// <param name="dateTimeOffset">The date and time to convert.</param>
    /// <returns>The date and time in the destination time zone.</returns>
    public static DateTimeOffset ToUsCentralTime(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ConvertTime(GetUsCentralTimeZoneInfo());
    }

    /// <summary>
    /// Windows uses the system registry to fetch time zone information.
    /// Linux instead has the trusty old tz database, which names time zones differently.
    /// I don't recall how it is named on a Mac. We shouldn't be deploying to a Mac docker host anyway. Panic if we do.
    /// </summary>
    /// <returns>TimeZoneInfo for Central Time</returns>
    private static TimeZoneInfo GetUsCentralTimeZoneInfo()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
        }

        throw new NotImplementedException("I don't know how to do a timezone lookup on a Mac.");
    }
}