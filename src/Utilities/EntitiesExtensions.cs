namespace NetDaemon.Utilities;

/// <summary>
/// Extensions for <see cref="IEntities"/>.
/// </summary>
public static class EntitiesExtensions
{
    /// <summary>
    /// Returns if anyone is actively home.
    /// </summary>
    public static bool IsAnyoneHome(this IEntities entities)
        => entities.Person.Owen.IsHome() || entities.Person.Allison.IsHome();
}