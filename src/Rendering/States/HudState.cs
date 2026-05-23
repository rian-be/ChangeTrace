using ChangeTrace.Rendering.Hud;

namespace ChangeTrace.Rendering.States;

/// <summary>
/// Aggregated HUD state for current render frame.
/// </summary>
internal record HudState(
    PlaybackHud Playback,
    InteractionHud Interaction,
    StatisticsHud Statistics,
    IReadOnlyList<LeaderboardEntry> Leaderboard);