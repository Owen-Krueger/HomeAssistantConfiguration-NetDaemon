using NetDaemon.HassModel.Entities;

namespace NetDaemon.Extensions;

/// <summary>
/// Extensions for <see cref="LockEntity"/>.
/// </summary>
public static class LockExtensions
{
    /// <summary>
    /// Returns if the entity is locked.
    /// </summary>
    public static bool IsLocked(this LockEntity lockEntity)
        => lockEntity.State == "locked";
    
    /// <summary>
    /// Returns if the entity is locked.
    /// </summary>
    public static bool IsLocked(this EntityState<LockAttributes>? lockEntity)
        => lockEntity?.State == "locked";
}