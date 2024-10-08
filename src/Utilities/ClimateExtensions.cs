namespace NetDaemon.Utilities;

/// <summary>
/// Extensions for <see cref="ClimateEntity"/>.
/// </summary>
public static class ClimateExtensions
{
    /// <summary>
    /// Returns if the thermostat's mode is set to "heat".
    /// </summary>
    public static bool IsHeatMode(this ClimateEntity climateEntity)
        => climateEntity.State is "heat";
}