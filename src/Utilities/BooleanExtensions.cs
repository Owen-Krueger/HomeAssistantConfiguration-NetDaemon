namespace NetDaemon.Utilities;

/// <summary>
/// Extensions for <see cref="bool"/>.
/// </summary>
public static class BooleanExtensions
{
    /// <summary>
    /// Gets boolean as string value (on/off).
    /// </summary>
    public static string GetOnOffString(this bool value)
        => value ? "On" : "Off";

    /// <summary>
    /// Gets on/off string value from string state. Returns "Unknown" if unable to parse bool from state.
    /// </summary>
    public static string GetOnOffStringFromState(this string? state)
    {
        if (state is null || !bool.TryParse(state, out var stateBool))
        {
            return "Unknown";
        }

        return stateBool.GetOnOffString();
    }
}