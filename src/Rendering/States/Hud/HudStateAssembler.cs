using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.States.Hud;

/// <summary>
/// Builds the aggregated HUD state used by the renderer.
/// </summary>
internal sealed class HudStateAssembler
{
    /// <summary>
    /// Creates a complete HUD snapshot from playback, interaction,
    /// statistics, and leaderboard state.
    /// </summary>
    public HudState Assemble(
        PlayerDiagnostics diagnostics,
        ICameraController cameraController,
        LayoutMode layoutMode,
        SceneNode? hoveredNode,
        HoveredPodHud? hoveredPod,
        int activeAvatarsCount,
        int totalNodeCount,
        IReadOnlyList<ExtensionStat> extensions,
        IReadOnlyList<LeaderboardEntry> leaderboard)
    {
        var timeSpan =
            TimeSpan.FromSeconds(
                diagnostics.PositionSeconds);

        var speedDirection =
            diagnostics.Direction == PlaybackDirection.Backward
                ? "-"
                : "";

        var speedRamp =
            diagnostics.IsRamping
                ? "↗"
                : "";

        var playback =
            new PlaybackHud(
                timeSpan.ToString(@"hh\:mm\:ss"),
                diagnostics.CurrentDateLabel,
                diagnostics.ElapsedDays,
                $"{speedDirection}{diagnostics.CurrentSpeed:F2}×{speedRamp}",
                (float)diagnostics.Progress,
                diagnostics.IsRamping,
                diagnostics.State,
                cameraController.Mode,
                layoutMode);

        var interaction =
            new InteractionHud(
                hoveredNode?.Id,
                hoveredNode?.LastAuthor,
                hoveredNode?.LastCommit,
                hoveredPod);

        var statistics =
            new StatisticsHud(
                activeAvatarsCount,
                totalNodeCount,
                diagnostics.EventsFired,
                diagnostics.TotalEvents,
                diagnostics.LoopCount,
                extensions);

        return new HudState(
            playback,
            interaction,
            statistics,
            leaderboard);
    }
}