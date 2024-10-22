using NetDaemon.HassModel.Entities;
using NetDaemon.Models.Enums;

namespace NetDaemon.Extensions;

/// <summary>
/// Extensions for <see cref="PersonEntity"/>.
/// </summary>
public static class PersonExtensions
{
    /// <summary>
    /// Returns if the person's state is "home".
    /// </summary>
    public static bool IsHome(this PersonEntity person)
        => person.GetEnumFromState(PersonStateEnum.Away) == PersonStateEnum.Home;

    /// <summary>
    /// Returns if the person's state is "home".
    /// </summary>
    public static bool IsHome(this EntityState<PersonAttributes>? person)
        => person?.GetEnumFromState(PersonStateEnum.Away) == PersonStateEnum.Home;
}