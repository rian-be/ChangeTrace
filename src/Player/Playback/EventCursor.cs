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

    /// <summary>Event at current cursor position (clamped to bounds), or null if empty.</summary>
    public TraceEvent? CurrentEvent => _events.Count > 0 ? _events[Math.Clamp(Index, 0, _events.Count - 1)] : null;

    /// <summary>Gets event at a specific timeline index.</summary>
    public TraceEvent GetEventAt(int index) => _events[index];

    /// <summary>First event in the timeline, or null if empty.</summary>
    public TraceEvent? FirstEvent => _events.Count > 0 ? _events[0] : null;
    
    /// <summary>Last event in the timeline, or null if empty.</summary>
    public TraceEvent? LastEvent => _events.Count > 0 ? _events[^1] : null;

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
        var (startIndex, count) = DrainForwardRange(virtualNow);
        if (count == 0)
            return [];

        var batch = new List<TraceEvent>(count);
        for (var i = 0; i < count; i++)
            batch.Add(_events[startIndex + i]);

        return batch;
    }

    /// <summary>Drains forward and returns the drained range without allocating a batch list.</summary>
    public (int StartIndex, int Count) DrainForwardRange(double virtualNow)
    {
        var startIndex = Index;
        while (Index < _events.Count && _events[Index].TimeForPlayback <= virtualNow)
            Index++;

        return (startIndex, Index - startIndex);
    }

    /// <summary>Drains and returns all events from current cursor down to <paramref name="virtualNow"/> while retreating cursor.</summary>
    public IReadOnlyList<TraceEvent> DrainBackward(double virtualNow)
    {
        var (startIndex, count) = DrainBackwardRange(virtualNow);
        if (count == 0)
            return [];

        var batch = new List<TraceEvent>(count);
        for (var i = 0; i < count; i++)
            batch.Add(_events[startIndex - i]);

        return batch;
    }

    /// <summary>Drains backward and returns the drained range without allocating a batch list.</summary>
    public (int StartIndex, int Count) DrainBackwardRange(double virtualNow)
    {
        var startIndex = Index;
        while (Index >= 0 && _events[Index].TimeForPlayback >= virtualNow)
            Index--;

        return (startIndex, startIndex - Index);
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

    /// <summary>Binary search for first event at or after target timestamp.</summary>
    private int BinarySearch(double target)
    {
        int lo = 0, hi = _events.Count - 1, result = _events.Count;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            if (_events[mid].TimeForPlayback >= target)
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
