using System.Text.Json.Serialization;

namespace NetDaemon.Models.Climate;

/// <summary>
/// An item from a weather's forecast service call.
/// </summary>
public class WeatherForecastItem
{
    /// <summary>
    /// The date/time of the forecast.
    /// </summary>
    [JsonPropertyName("datetime")]
    public DateTimeOffset DateTime { get; set; }

    /// <summary>
    /// The temperature at the above date/time.
    /// </summary>
    [JsonPropertyName("temperature")]
    public int Temperature { get; set; }

    /// <summary>
    /// The humidity outside.
    /// </summary>
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}