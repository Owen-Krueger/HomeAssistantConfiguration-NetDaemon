namespace NetDaemon.Models.Climate;

/// <summary>
/// The state of the house (climate-wise).
/// </summary>
public enum ThermostatState
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