using ChangeTrace.Player;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering.Hud;

/// <summary>
/// Builds <see cref="HudState"/> instances from player diagnostics and scene data.
/// </summary>
/// <remarks>
/// Responsible for formatting time, speed, progress,
/// and assembling final HUD state used by rendering layer.
/// </remarks>
internal static class HudBuilder
{
    /// <summary>
    /// Creates new <see cref="HudState"/> based on current playback diagnostics and scene metrics.
    /// </summary>
    /// <param name="diagnostics">Current player diagnostics.</param>
    /// <param name="activeActors">Number of active actors.</param>
    /// <param name="totalNodes">Total number of nodes in scene.</param>
    /// <param name="leaderboard">Leaderboard entries.</param>
    /// <returns>Constructed <see cref="HudState"/>.</returns>
    internal static HudState Build(
        PlayerDiagnostics diagnostics,
        int activeActors,
        int totalNodes,
        IReadOnlyList<LeaderboardEntry> leaderboard)
    {
        return new HudState(
            TimeLabel:    FormatTime(diagnostics.PositionSeconds),
            SpeedLabel:   FormatSpeed(diagnostics.CurrentSpeed, diagnostics.IsRamping),
            Progress:     Clamp01((float)diagnostics.Progress),
            EventsFired:  diagnostics.EventsFired,
            TotalEvents:  diagnostics.TotalEvents,
            ActiveActors: activeActors,
            TotalNodes:   totalNodes,
            LoopCount:    diagnostics.LoopCount,
            IsRamping:    diagnostics.IsRamping,
            Leaderboard:  leaderboard
        );
    }

    /// <summary>
    /// Formats playback time in mm:ss or hh:mm:ss format.
    /// </summary>
    /// <param name="seconds">Playback position in seconds.</param>
    /// <returns>Formatted time string.</returns>
    private static string FormatTime(double seconds)
    {
        if (seconds < 0) seconds = 0;

        var ts = TimeSpan.FromSeconds(seconds);

        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    /// <summary>
    /// Formats playback speed label.
    /// </summary>
    /// <param name="speed">Current playback speed.</param>
    /// <param name="ramping">Indicates whether speed is ramping.</param>
    /// <returns>Formatted speed label.</returns>
    private static string FormatSpeed(double speed, bool ramping)
        => ramping
            ? $"{speed:F2}×↗"
            : $"{speed:F2}×";

    /// <summary>
    /// Clamps value to [0, 1] range.
    /// </summary>
    /// <param name="value">Input value.</param>
    /// <returns>Clamped value.</returns>
    private static float Clamp01(float value)
        => value < 0f ? 0f : value > 1f ? 1f : value;
}