namespace NetDaemon.Models.Enums;

/// <summary>
/// The state of the house.
/// </summary>
public enum HomeStateEnum
{
    /// <summary>
    /// Someone is home and temperature should follow a schedule.
    /// </summary>
    Home,

    /// <summary>
    /// Nobody is home and temperature should be set based on distance from home.
    /// </summary>
    Away
}