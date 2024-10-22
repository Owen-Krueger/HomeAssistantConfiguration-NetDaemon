using System.Collections.Generic;

namespace NetDaemon.Utilities;

/// <summary>
/// Extensions for working with <see cref="IDisposable"/> triggers.
/// </summary>
public static class TriggerUtilities
{
    /// <summary>
    /// Disposes of triggers and ensures list is empty when complete.
    /// </summary>
    public static List<IDisposable> DisposeTriggers(this List<IDisposable> triggers)
    {
        triggers.ForEach(x => x.Dispose());
        return [];
    }
    
    /// <summary>
    /// Updates automation triggers. If <see cref="triggersActive"/> is true and no triggers are set, gets triggers
    /// from <see cref="setUpTriggersFunction"/>. If <see cref="triggers"/> is false and triggers are set, all
    /// triggers are disposed.
    /// </summary>
    /// <param name="triggers">Existing trigger list.</param>
    /// <param name="triggersActive">Whether the triggers should be active.</param>
    /// <param name="setUpTriggersFunction">Function to get triggers from, if they should be on.</param>
    /// <returns>The updated triggers list.</returns>
    public static List<IDisposable> UpdateAutomationTriggers(List<IDisposable> triggers,
        bool triggersActive, Func<List<IDisposable>> setUpTriggersFunction)
    {
        triggers = triggersActive switch
        {
            true when triggers.Count == 0 => setUpTriggersFunction(),
            false when triggers.Count > 0 => triggers.DisposeTriggers(),
            _ => triggers
        };

        return triggers;
    }
}