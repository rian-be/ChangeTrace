namespace ChangeTrace.Rendering.Hud;

/// <summary>
/// Represents single entry in HUD leaderboard.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="LeaderboardEntry"/> captures activity level of single actor
/// within current playback session.
/// </para>
/// <para>
/// Entries are typically ordered descending by <see cref="EventCount"/> before
/// being rendered in HUD.
/// </para>
/// </remarks>
/// <param name="Actor">
/// The display name of actor.
/// </param>
/// <param name="EventCount">
/// The total number of events attributed to actor during current playback scope.
/// </param>
internal sealed record LeaderboardEntry(
    string Actor,
    int EventCount
);
