namespace NetDaemon.Models.Climate;

/// <summary>
/// Represents a <see cref="Temperature"/> that, when set, takes <see cref="MinutesToDesired"/> minutes to get back
/// to the desired temperature.
/// </summary>
public record TimingThreshold
{
    /// <summary>
    /// Initializes an empty <see cref="TimingThreshold"/> object.
    /// </summary>
    public TimingThreshold() { }

    /// <summary>
    /// Instantiates an <see cref="TimingThreshold"/> object, based on a temperature and minutes to
    /// desired temperature.
    /// </summary>
    public TimingThreshold(double temperature, double minutesToDesired)
    {
        Temperature = temperature;
        MinutesToDesired = minutesToDesired;
    }
    
    /// <summary>
    /// Temperature to set the thermostat to.
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// Time to desired temperature from <see cref="Temperature"/>.
    /// </summary>
    public double MinutesToDesired { get; set; }

    /// <summary>
    /// Formats string describing threshold.
    /// </summary>
    public override string ToString()
        => $"{Temperature} - {MinutesToDesired} minutes";
}