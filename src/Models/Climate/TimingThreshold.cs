namespace NetDaemon.Models.Climate;

public record TimingThreshold
{
    public double Temperature { get; set; }

    public double MinutesToDesired { get; set; }
}