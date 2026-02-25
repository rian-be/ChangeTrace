using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Events;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Playback;

/// <summary>
/// Cursor for scanning timeline events forward or backward.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Maintains an index within a read-only timeline of <see cref="TraceEvent"/>.</item>
/// <item>Supports seeking to virtual timestamps, draining batches of events, and single-step iteration.</item>
/// <item>Does not manage timing or speed; purely navigational.</item>
/// <item>Registered as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class EventCursor : IEventCursor
{
    private readonly IReadOnlyList<TraceEvent> _events;

    /// <summary>Current cursor index in the event list.</summary>
    public int Index { get; private set; }

    /// <summary>True if cursor is at the end of events.</summary>
    public bool AtEnd => Index >= _events.Count;

    /// <summary>True if cursor is before the start of events.</summary>
    public bool AtStart => Index < 0;

    /// <summary>Total number of events in timeline.</summary>
    public int TotalEvents => _events.Count;

    /// <summary>Initializes the cursor with a timeline of events.</summary>
    /// <param name="events">Read-only list of timeline events.</param>
    internal EventCursor(IReadOnlyList<TraceEvent> events)
    {
        _events = events ?? throw new ArgumentNullException(nameof(events));
        Index = 0;
    }

    /// <summary>Resets cursor to first event.</summary>
    public void ResetToStart() => Index = 0;

    /// <summary>Resets cursor to last event.</summary>
    public void ResetToEnd() => Index = _events.Count - 1;

    /// <summary>Seeks cursor to first event at or after <paramref name="virtualSeconds"/>.</summary>
    /// <param name="virtualSeconds">Target virtual timestamp.</param>
    public void SeekTo(double virtualSeconds)
    {
        Index = Math.Min(BinarySearch(virtualSeconds), _events.Count - 1);
        if (Index < 0) Index = 0;
    }

    /// <summary>Drains and returns all events up to <paramref name="virtualNow"/> while advancing cursor.</summary>
    public IReadOnlyList<TraceEvent> DrainForward(double virtualNow)
    {
        var batch = new List<TraceEvent>();
        while (Index < _events.Count && TimeOf(Index) <= virtualNow)
            batch.Add(_events[Index++]);
        return batch;
    }

    /// <summary>Drains and returns all events from current cursor down to <paramref name="virtualNow"/> while retreating cursor.</summary>
    public IReadOnlyList<TraceEvent> DrainBackward(double virtualNow)
    {
        var batch = new List<TraceEvent>();
        while (Index >= 0 && TimeOf(Index) >= virtualNow)
            batch.Add(_events[Index--]);
        return batch;
    }

    /// <summary>Steps cursor forward by one event.</summary>
    /// <returns>Tuple of event and whether cursor moved.</returns>
    public (TraceEvent? Event, bool Moved) TryStepForward()
    {
        if (Index >= _events.Count) return (null, false);
        var evt = _events[Index++];
        return (evt, true);
    }

    /// <summary>Steps cursor backward by one event.</summary>
    /// <returns>Tuple of event and whether cursor moved.</returns>
    public (TraceEvent? Event, bool Moved) TryStepBackward()
    {
        if (Index <= 0) return (null, false);
        var evt = _events[--Index];
        return (evt, true);
    }

    /// <summary>Gets timestamp of event at given index, or <c>double.MaxValue</c> if out of range.</summary>
    private double TimeOf(int idx)
    {
        if (idx < 0 || idx >= _events.Count)
            return double.MaxValue;

        return _events[idx].TimeForPlayback;
    }
    
    /// <summary>Binary search for first event at or after target timestamp.</summary>
    private int BinarySearch(double target)
    {
        int lo = 0, hi = _events.Count - 1, result = _events.Count;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            if (TimeOf(mid) >= target)
            {
                result = mid;
                hi = mid - 1;
            }
            else
            {
                lo = mid + 1;
            }
        }
        return result;
    }
}