using ChangeTrace.Core.Events;
using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Timelines;

/// <summary>
/// Provides utilities for normalizing a <see cref="Timeline"/> so that
/// its events are ordered and scaled to a target duration.
/// </summary>
internal static class TimelineNormalizer
{
    public static Result Normalize(Timeline timeline, double targetDurationSeconds = 300.0)
    {
        var events = timeline.EventsSpan;

        if (events.Length == 0)
            return Result.Failure("Cannot normalize empty timeline");

        var list = timeline.Events as List<TraceEvent>;
        list!.Sort((a, b) => a.Core.Timestamp.CompareTo(b.Core.Timestamp));

        var baseTime = list[0].Core.Timestamp;
        var span = list[^1].Core.Timestamp.UnixSeconds - baseTime.UnixSeconds;

        var scale = span > 1e-9 ? targetDurationSeconds / span : 1.0;

        for (int i = 0; i < list.Count; i++)
        {
            list[i] = list[i].ComputeRelative(baseTime, scale);
        }

        return Result.Success();
    }
}