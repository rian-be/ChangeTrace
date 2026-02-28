using ChangeTrace.Rendering.Hud;

namespace ChangeTrace.Rendering.States;

/// <summary>
/// Represents current state of HeadsUp Display (HUD) in rendering.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HudState"/> is  snapshot style immutable record describing all UI-facing
/// metrics required to render the HUD at a specific frame in time.
/// </para>
/// <para>
/// It aggregates playback timing, simulation progress, actor/node statistics,
/// loop information, and leaderboard standings.
/// </para>
/// </remarks>
/// <param name="TimeLabel">
/// Formatted playback time label (e.g., "00:03:42").
/// </param>
/// <param name="SpeedLabel">
/// Playback speed indicator (e.g., "3.00×↗" for ramping, or "3.00×" for steady speed).
/// </param>
/// <param name="Progress">
/// Normalized playback progress in the range 0.0–1.0.
/// </param>
/// <param name="EventsFired">
/// Number of events that have been processed so far.
/// </param>
/// <param name="TotalEvents">
/// Total number of events in the trace.
/// </param>
/// <param name="ActiveActors">
/// Number of currently active actors in the scene.
/// </param>
/// <param name="TotalNodes">
/// Total number of nodes currently present in the visualization.
/// </param>
/// <param name="LoopCount">
/// Number of completed playback loops.
/// </param>
/// <param name="IsRamping">
/// Indicates whether playback speed is currently ramping.
/// </param>
/// <param name="Leaderboard">
/// Ordered list of <see cref="LeaderboardEntry"/> objects representing actor rankings.
/// </param>
internal sealed record HudState(
    string TimeLabel,
    string SpeedLabel,
    float Progress,
    int EventsFired,
    int TotalEvents,
    int ActiveActors,
    int TotalNodes,
    int LoopCount,
    bool IsRamping,
    IReadOnlyList<LeaderboardEntry> Leaderboard
);