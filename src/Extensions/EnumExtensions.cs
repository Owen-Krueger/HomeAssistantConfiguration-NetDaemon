using NetDaemon.HassModel.Entities;

namespace NetDaemon.Extensions;

/// <summary>
/// Extensions for <see cref="Enum"/>.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets <see cref="TEnum"/> value from <see cref="Entity"/> state. Returns default enum value if unable to parse.
    /// </summary>
    public static TEnum GetEnumFromState<TEnum>(this Entity entity)
        where TEnum : Enum
        => GetEnumFromState(entity.EntityState, (TEnum)default!);
    
    /// <summary>
    /// Gets <see cref="TEnum"/> value from <see cref="Entity"/> state. Returns <see cref="defaultEnum"/>
    /// if unable to parse.
    /// </summary>
    public static TEnum GetEnumFromState<TEnum>(this Entity entity, TEnum defaultEnum)
        where TEnum : Enum
        => GetEnumFromState(entity.EntityState, defaultEnum);

    /// <summary>
    /// Gets <see cref="TEnum"/> value from <see cref="EntityState{TAttributes}"/> state. Returns default enum
    /// value if unable to parse.
    /// </summary>
    public static TEnum GetEnumFromState<TEnum>(this EntityState? entityState)
        where TEnum : Enum
        => GetEnumFromState(entityState, (TEnum)default!);
    
    /// <summary>
    /// Gets <see cref="TEnum"/> value from <see cref="EntityState{TAttributes}"/> state. Returns
    /// <see cref="defaultEnum"/> enum value if unable to parse.
    /// </summary>
    public static TEnum GetEnumFromState<TEnum>(this EntityState? entityState, TEnum defaultEnum)
        where TEnum : Enum
    {
        if (entityState?.State is null || !Enum.TryParse(typeof(TEnum), entityState.State, true, out var state))
        {
            return defaultEnum;
        }

        return (TEnum)state;
    }

    /// <summary>
    /// Gets the lowercase string representation of the <see cref="TEnum"/>.
    /// </summary>
    public static string ToStringLowerCase<TEnum>(this TEnum value)
        where TEnum : Enum
        => value.ToString().ToLower();
}