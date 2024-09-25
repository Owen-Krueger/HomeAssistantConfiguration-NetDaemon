namespace NetDaemon.Utilities;

public static class LockExtensions
{
    public static bool IsLocked(this LockEntity lockEntity)
        => lockEntity.State == "locked";
}