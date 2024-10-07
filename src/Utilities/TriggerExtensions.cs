using System.Collections.Generic;

namespace NetDaemon.Utilities;

/// <summary>
/// Extensions for working with <see cref="IDisposable"/> triggers.
/// </summary>
public static class TriggerExtensions
{
    /// <summary>
    /// Disposes of triggers and ensures list is empty when complete.
    /// </summary>
    public static List<IDisposable> DisposeTriggers(this List<IDisposable> triggers)
    {
        triggers.ForEach(x => x.Dispose());
        return [];
    }
}