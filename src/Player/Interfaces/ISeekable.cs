using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;

namespace ChangeTrace.Player.Interfaces;

/// <summary>
/// Exposes timeline position and seeking operations for playback.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides current position, total duration, and normalized progress.</item>
/// <item>Supports seeking to an absolute timestamp or relative delta.</item>
/// <item>Notifies subscribers via <see cref="OnProgress"/> when progress changes.</item>
/// </list>
/// </remarks>
internal interface ISeekable
{
    /// <summary>Current playback position in seconds.</summary>
    double PositionSeconds { get; }

    /// <summary>Total timeline duration in seconds.</summary>
    double DurationSeconds { get; }

    /// <summary>Normalized progress (0.0 – 1.0) through timeline.</summary>
    double Progress { get; }

    /// <summary>
    /// Seeks to specific timestamp in the timeline.
    /// </summary>
    /// <param name="position">Target timestamp.</param>
    /// <returns>Result indicating success or failure.</returns>
    Result Seek(Timestamp position);    

    /// <summary>
    /// Seeks relative to current position by delta seconds.
    /// </summary>
    /// <param name="deltaSeconds">Time offset in seconds (positive or negative).</param>
    /// <returns>Result indicating success or failure.</returns>
    Result SeekRelative(double deltaSeconds);

    /// <summary>
    /// Fired whenever progress changes.
    /// Parameter is current normalized progress (0.0 – 1.0).
    /// </summary>
    event Action<double>? OnProgress;
}