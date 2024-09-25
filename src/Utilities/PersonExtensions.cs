using NetDaemon.HassModel.Entities;

namespace NetDaemon.Utilities;

/// <summary>
/// Extensions for <see cref="PersonEntity"/>.
/// </summary>
public static class PersonExtensions
{
    /// <summary>
    /// Returns if the person's state is "home".
    /// </summary>
    public static bool IsHome(this PersonEntity person)
        => person.State == "home";

    /// <summary>
    /// Returns if the person's state is "home".
    /// </summary>
    public static bool IsHome(this EntityState<PersonAttributes>? person)
        => person?.State == "home";
}