using System.Collections.Generic;
using System.Linq;
using NetDaemon.Models.Climate;

namespace NetDaemon.Utilities;

/// <summary>
/// Utilities used in climate automations.
/// </summary>
public static class ClimateUtilities
{
    /// <summary>
    /// Returns if the <see cref="ClimateEntity"/>'s mode is set to "heat".
    /// </summary>
    public static bool IsHeatMode(this ClimateEntity climateEntity)
        => climateEntity.State is "heat";
    
    public static List<TimingThreshold> GetTimingThresholds(double desiredTemperature, bool isHeatMode)
    {
        List<TimingThreshold> temperatureTimes = [];
        var factor = isHeatMode ? -1 : 1;

        temperatureTimes.Add(new TimingThreshold(desiredTemperature, 0));
        for (var i = 1; i <= 8; i++)
        {
            var temperature = desiredTemperature + factor * i;
            temperatureTimes.Add(GetTemperatureTiming(temperature, desiredTemperature, isHeatMode));
        }

        // These should already be in order, but I'm paranoid.
        temperatureTimes = temperatureTimes.OrderBy(x => x.MinutesToDesired).ToList();
        
        return temperatureTimes;
    }
    
    /// <summary>
    /// Gets amount of time to arrive at the desired temperature based on how far away the temperature currently is
    /// (the offset).
    /// </summary>
    private static TimingThreshold GetTemperatureTiming(double temperature, double desiredTemperature, bool isHeatMode)
    {
        return new TimingThreshold(temperature, Math.Abs(temperature - desiredTemperature) * 10);

        /*const int houseVolume = 23_562; // 2,618 liveable x 9 ft ceilings
        const double airDensity = 0.075;
        const double heatOfAir = 0.24;

        if (isHeatMode)
        {
            const int heatingCapacity = 60_000;
            return new TimingThreshold(temperature,
                houseVolume * (desiredTemperature - temperature) * airDensity * heatOfAir / heatingCapacity);
        }

        const int coolingCapacity = 24_000;
        return new TimingThreshold(temperature,
            houseVolume * (temperature - desiredTemperature) * airDensity * heatOfAir / coolingCapacity);*/
    }
}