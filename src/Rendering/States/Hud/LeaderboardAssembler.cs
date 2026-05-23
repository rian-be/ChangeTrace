using ChangeTrace.Rendering.Hud;

namespace ChangeTrace.Rendering.States.Hud;

/// <summary>
/// Tracks contributor activity and builds leaderboard snapshots.
/// </summary>
internal sealed class LeaderboardAssembler
{
    private readonly Dictionary<string, int> _actorCommitCounts = new();

    private LeaderboardEntry[] _cachedLeaderboard = [];
    private bool _dirty = true;

    /// <summary>
    /// Records activity for a contributor and invalidates cached rankings.
    /// </summary>
    public void RecordActorEvent(string actor)
    {
        _actorCommitCounts[actor] =
            _actorCommitCounts.GetValueOrDefault(actor) + 1;

        _dirty = true;
    }

    /// <summary>
    /// Clears all leaderboard state and cached results.
    /// </summary>
    public void Reset()
    {
        _actorCommitCounts.Clear();

        _cachedLeaderboard = [];
        _dirty = true;
    }

    /// <summary>
    /// Builds or returns the cached contributor leaderboard.
    /// </summary>
    public IReadOnlyList<LeaderboardEntry> Assemble()
    {
        if (!_dirty)
            return _cachedLeaderboard;

        _cachedLeaderboard =
            _actorCommitCounts
                .OrderByDescending(x => x.Value)
                .Take(12)
                .Select(x => new LeaderboardEntry(x.Key, x.Value))
                .ToArray();

        _dirty = false;

        return _cachedLeaderboard;
    }
}