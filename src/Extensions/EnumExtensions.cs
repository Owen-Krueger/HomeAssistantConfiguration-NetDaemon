namespace NetDaemon.Extensions;

/// <summary>
/// Extensions for <see cref="Enum"/>.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets <see cref="TEnum"/> value from <see cref="InputSelectEntity"/> state. Returns default enum value if unable to parse.
    /// </summary>
    public static TEnum GetEnumFromState<TEnum>(this InputSelectEntity entity)
    where TEnum : Enum
    {
        if (entity.State is null || !Enum.TryParse(typeof(TEnum), entity.State, false, out var state))
        {
            return default!;
        }

        return (TEnum)state;
    }
}