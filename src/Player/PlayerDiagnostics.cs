using ChangeTrace.Player.Enums;

namespace ChangeTrace.Player;

/// <summary>
/// Immutable diagnostics snapshot describing current player state.
/// </summary>
/// <remarks>
/// Captures runtime playback metrics including state, timing, speed,
/// progress, event statistics, and performance indicators.
/// </remarks>
internal sealed record PlayerDiagnostics(
    PlayerState State,
    PlaybackMode Mode,
    PlaybackDirection Direction,
    double CurrentSpeed,
    double TargetSpeed,
    bool IsRamping,
    double PositionSeconds,
    double DurationSeconds,
    double Progress,
    int EventsFired,
    int TotalEvents,
    int LoopCount,
    double WallElapsedSeconds,
    int TickCount,
    double AvgEventsPerTick
)
{
    /// <summary>
    /// Returns formatted diagnostic summary string.
    /// </summary>
    /// <returns>
    /// Human-readable one line representation of current playback metrics.
    /// </returns>
    public override string ToString() =>
        $"[{State}|{Mode}] {Direction} " +
        $"{CurrentSpeed:F2}×{(IsRamping ? "↗" : " ")} " +
        $"pos={PositionSeconds:F1}s/{DurationSeconds:F1}s ({Progress:P0}) " +
        $"events={EventsFired}/{TotalEvents} loops={LoopCount} " +
        $"ticks={TickCount} avg={AvgEventsPerTick:F2}ev/tick";
}