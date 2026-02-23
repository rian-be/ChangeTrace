using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Player.Playback;

/// <summary>
/// Timeline that supports seeking and progress reporting.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ISeekable"/> to allow absolute and relative seeking.</item>
/// <item>Reports <see cref="Progress"/> as a fraction 0–1 of timeline duration.</item>
/// <item>Does not manage playback state; use <see cref="IPlaybackTransport"/> for play/pause/stop.</item>
/// <item>Thread-safe via internal lock.</item>
/// <item>Registered as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class SeekableTimeline : ISeekable
{
    private readonly IVirtualClock _clock;
    private readonly IEventCursor _cursor;
    private readonly Lock _lock = new();

    /// <summary>Current position in seconds.</summary>
    public double PositionSeconds => _clock.VirtualNow;

    /// <summary>Total timeline duration in seconds.</summary>
    public double DurationSeconds { get; }

    /// <summary>Normalized progress 0–1.</summary>
    public double Progress => DurationSeconds > 1e-9
        ? Math.Clamp(_clock.VirtualNow / DurationSeconds, 0, 1)
        : 0;

    /// <summary>Fired whenever progress changes.</summary>
    public event Action<double>? OnProgress;

    /// <summary>Initializes timeline with clock, event cursor, and total duration.</summary>
    /// <param name="clock">Virtual clock to track timeline position.</param>
    /// <param name="cursor">Event cursor for timeline events.</param>
    /// <param name="durationSeconds">Total timeline duration in seconds.</param>
    public SeekableTimeline(IVirtualClock clock, IEventCursor cursor, double durationSeconds)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(cursor);

        _clock = clock;
        _cursor = cursor;
        DurationSeconds = durationSeconds;
    }

    /// <summary>Seek to absolute timeline position.</summary>
    /// <param name="position">Target timestamp.</param>
    /// <returns>Success or failure result.</returns>
    public Result Seek(Timestamp position)
        => SeekTo(position.UnixSeconds);

    /// <summary>Seek relative to current timeline position.</summary>
    /// <param name="deltaSeconds">Seconds to move (positive or negative).</param>
    /// <returns>Success or failure result.</returns>
    public Result SeekRelative(double deltaSeconds)
        => SeekTo(_clock.VirtualNow + deltaSeconds);

    private Result SeekTo(double targetSeconds)
    {
        lock (_lock)
        {
            if (_cursor.TotalEvents == 0)
                return Result.Failure("Timeline is empty.");

            targetSeconds = Math.Clamp(targetSeconds, 0, DurationSeconds);

            _clock.SnapPosition(targetSeconds);
            _cursor.SeekTo(targetSeconds);

            OnProgress?.Invoke(Progress);
        }
        return Result.Success();
    }

    /// <summary>
    /// Computes timeline duration from first and last event timestamps.
    /// </summary>
    /// <param name="timeline">Timeline to measure.</param>
    /// <returns>Total duration in seconds.</returns>
    private static double ComputeDuration(Timeline timeline)
    {
        if (timeline.Count == 0) return 0;
        var stats = timeline.GetStatistics();
        return stats.EndTime.UnixSeconds - stats.StartTime.UnixSeconds;
    }
}