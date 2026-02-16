using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Services;

/// <summary>
/// Tracks branch state during timeline construction.
/// Maintains history of branch existence, last commit, and timestamps to detect
/// branch creation and deletion events while building the timeline.
/// </summary>
internal sealed class BranchTracker
{
    private readonly Dictionary<string, BranchState> _states = new();

    /// <summary>
    /// Determines if a branch is newly created (not previously tracked).
    /// </summary>
    /// <param name="branchName">Name of the branch to check.</param>
    /// <returns>True if branch was not previously seen; otherwise false.</returns>
    public bool IsNew(string branchName) => !_states.ContainsKey(branchName);

    /// <summary>
    /// Updates the state of a branch with its latest commit information.
    /// </summary>
    /// <param name="branchName">Name of the branch to update.</param>
    /// <param name="sha">Latest commit SHA on the branch.</param>
    /// <param name="timestamp">Timestamp of the latest commit.</param>
    public void Update(string branchName, CommitSha sha, Timestamp timestamp) =>
        _states[branchName] = new BranchState(sha, timestamp);

    /// <summary>
    /// Identifies branches that have been deleted since last update.
    /// Removes deleted branches from tracking and returns their names and last commit SHA.
    /// </summary>
    /// <param name="currentBranches">Set of currently active branch names.</param>
    /// <returns>Collection of deleted branch names with their last commit SHA.</returns>
    public IEnumerable<(string Name, CommitSha LastSha)> GetDeleted(HashSet<string> currentBranches)
    {
        var deleted = _states.Keys
            .Where(b => !currentBranches.Contains(b))
            .Select(b => (b, _states[b].Sha))
            .ToList();

        foreach (var (name, _) in deleted)
        {
            _states.Remove(name);
        }
        return deleted;
    }

    /// <summary>
    /// Internal state record for a tracked branch.
    /// </summary>
    private record struct BranchState(CommitSha Sha, Timestamp Timestamp);
}