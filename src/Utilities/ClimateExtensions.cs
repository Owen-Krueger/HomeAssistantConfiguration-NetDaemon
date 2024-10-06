namespace NetDaemon.Utilities;

public static class ClimateExtensions
{
    public static bool IsHeatMode(this ClimateEntity climateEntity)
        => climateEntity.State is "heat";
}