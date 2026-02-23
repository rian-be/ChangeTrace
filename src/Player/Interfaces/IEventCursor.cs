using ChangeTrace.Core.Events;

namespace ChangeTrace.Player.Interfaces;

/// <summary>
/// Handles event dispatch and cursor management for playback.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides read only access to current event index and total events.</item>
/// <item>Supports forward and backward event drainage based on virtual time.</item>
/// <item>Enables stepping through events with feedback if cursor moved.</item>
/// <item>Supports seeking and resetting to start or end of event stream.</item>
/// </list>
/// </remarks>
internal interface IEventCursor
{
    /// <summary>Current cursor index in event list.</summary>
    int Index { get; }

    /// <summary>Total number of events available.</summary>
    int TotalEvents { get; }

    /// <summary>True if cursor is at the start of the event list.</summary>
    bool AtStart { get; }

    /// <summary>True if cursor is at the end of the event list.</summary>
    bool AtEnd { get; }

    /// <summary>
    /// Retrieves events forward from current cursor until given virtual time.
    /// </summary>
    /// <param name="virtualNow">Virtual time to drain events up to.</param>
    /// <returns>Read-only list of events drained.</returns>
    IReadOnlyList<TraceEvent> DrainForward(double virtualNow);

    /// <summary>
    /// Retrieves events backward from current cursor until given virtual time.
    /// </summary>
    /// <param name="virtualNow">Virtual time to drain events down to.</param>
    /// <returns>Read-only list of events drained.</returns>
    IReadOnlyList<TraceEvent> DrainBackward(double virtualNow);

    /// <summary>
    /// Steps cursor forward by one event.
    /// </summary>
    /// <returns>Tuple of event at new position (or null) and whether cursor moved.</returns>
    (TraceEvent? Event, bool Moved) TryStepForward();

    /// <summary>
    /// Steps cursor backward by one event.
    /// </summary>
    /// <returns>Tuple of event at new position (or null) and whether cursor moved.</returns>
    (TraceEvent? Event, bool Moved) TryStepBackward();

    /// <summary>Moves cursor to position corresponding to given virtual time.</summary>
    /// <param name="virtualSeconds">Target virtual time in seconds.</param>
    void SeekTo(double virtualSeconds);

    /// <summary>Resets cursor to start of event list.</summary>
    void ResetToStart();

    /// <summary>Resets cursor to end of event list.</summary>
    void ResetToEnd();
}