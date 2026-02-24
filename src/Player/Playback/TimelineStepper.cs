using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Results;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Playback;

/// <summary>
/// Provides single step navigation through timeline.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Moves cursor forward or backward one event at a time.</item>
/// <item>Updates <see cref="IVirtualClock"/> position to match current event.</item>
/// <item>Optionally invokes a callback <c>onEvent</c> on each step.</item>
/// </list>
/// <param name="cursor">Event cursor to navigate timeline.</param>
/// <param name="clock">Virtual clock to update position.</param>
/// <param name="onEvent">Optional callback invoked after each step.</param>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TimelineStepper(IEventCursor cursor, IVirtualClock clock, Action<TraceEvent>? onEvent = null)
    : IStepper
{
    private readonly IEventCursor _cursor = cursor ?? throw new ArgumentNullException(nameof(cursor));
    private readonly IVirtualClock _clock = clock ?? throw new ArgumentNullException(nameof(clock));

    /// <summary>
    /// Steps timeline forward by one event.
    /// </summary>
    /// <returns>Success if cursor moved, failure if at end or timeline empty.</returns>
    public Result StepForward()
    {
        if (_cursor.TotalEvents == 0)
            return Result.Failure("Timeline is empty.");

        var (evt, moved) = _cursor.TryStepForward();
        if (!moved) return Result.Failure("Already at end.");

        _clock.SnapPosition(evt!.Timestamp.UnixSeconds);
        onEvent?.Invoke(evt);
        return Result.Success();
    }

    /// <summary>
    /// Steps timeline backward by one event.
    /// </summary>
    /// <returns>Success if cursor moved, failure if at beginning or timeline empty.</returns>
    public Result StepBackward()
    {
        if (_cursor.TotalEvents == 0)
            return Result.Failure("Timeline is empty.");

        var (evt, moved) = _cursor.TryStepBackward();
        if (!moved) return Result.Failure("Already at beginning.");

        _clock.SnapPosition(evt!.Timestamp.UnixSeconds);
        onEvent?.Invoke(evt);
        return Result.Success();
    }
}