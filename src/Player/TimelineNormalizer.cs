using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Player;

/// <summary>
/// Helper to normalize event timestamps into a relative timeline starting at 0.
/// Updates DurationSeconds for the player.
/// </summary>
internal static class TimelineNormalizer
{
    public static void NormalizeTimestamps(IList<TraceEvent>? events, out double durationSeconds)
    {
        if (events == null || events.Count == 0)
        {
            durationSeconds = 0;
            return;
        }
            
        Timestamp minTs = events.Min(e => e.Timestamp);

        foreach (var evt in events)
        {
            long relativeSeconds = evt.Timestamp.UnixSeconds - minTs.UnixSeconds;
                
            var tsResult = Timestamp.Create(relativeSeconds);
            if (!tsResult.IsSuccess)
                throw new InvalidOperationException($"Cannot normalize timestamp: {tsResult.Error}");

            evt.Timestamp = tsResult.Value;
        }

        durationSeconds = events.Max(e => e.Timestamp.UnixSeconds);
    }
}