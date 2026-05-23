using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Utils;

namespace ChangeTrace.Core.Services;

/// <summary>
/// Tracks branch state during timeline construction.
/// Maintains history of branch existence, last commit, and timestamps to detect
/// branch creation and deletion events while building the timeline.
/// </summary>
internal sealed class BranchTracker
{
    private readonly Dictionary<string, BranchState> _states = new(StringComparer.OrdinalIgnoreCase);
    
    /// <summary>
    /// Determines if a branch is newly created (not previously tracked).
    /// </summary>
    /// <param name="branchName">Name of the branch to check.</param>
    /// <returns>True if branch was not previously seen; otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNew(string branchName) => !_states.ContainsKey(branchName);

    /// <summary>
    /// Updates the state of a branch with its latest commit information.
    /// </summary>
    /// <param name="branchName">Name of the branch to update.</param>
    /// <param name="sha">Latest commit SHA on the branch.</param>
    /// <param name="timestamp">Timestamp of the latest commit.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(string branchName, CommitSha sha, Timestamp timestamp)
    {
        ref var state = ref CollectionsMarshal.GetValueRefOrAddDefault(_states, branchName, out _);
        state = new BranchState(sha, timestamp);
    }

    /// <summary>
    /// Identifies branches that have been deleted since last update.
    /// Removes deleted branches from tracking and returns their names and last commit SHA.
    /// </summary>
    /// <param name="currentBranches">Set of currently active branch names.</param>
    /// <param name="deletedBuffer"></param>
    /// <returns>Collection of deleted branch names with their last commit SHA.</returns>
    public int GetDeleted(HashSet<string> currentBranches, Span<(string Name, CommitSha LastSha)> deletedBuffer)
    {
        if (deletedBuffer.IsEmpty)
            return 0;

        int count = 0;
        List<string>? toRemove = null;

        foreach (var kvp in _states)
        {
            if (currentBranches.Contains(kvp.Key)) continue;
            if (count < deletedBuffer.Length)
            {
                deletedBuffer[count] = (kvp.Key, kvp.Value.Sha);
                count++;
                    
                toRemove ??= [];
                toRemove.Add(kvp.Key);
            }
            else
            {
                break; // Buffer full
            }
        }

        if (toRemove == null) return count;
        foreach (string name in toRemove)
            _states.Remove(name);

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryUpdate(string branchName, CommitSha? sha, Timestamp timestamp)
    {
        ref var state = ref CollectionsMarshal.GetValueRefOrAddDefault(_states, branchName, out bool existed);
        if (sha != null) state = new BranchState(sha, timestamp);
        return !existed;
    }
    
    /// <summary>
    /// Gets deleted branches with their last commit SHA and timestamp using ArrayPool.
    /// Returns a pooled collection that must be disposed after use.
    /// </summary>
    /// <param name="currentBranches">Set of currently active branch names.</param>
    /// <returns>Pooled collection of deleted branches with SHA and timestamp.</returns>
    public PooledDeletedBranches GetDeletedPooled(HashSet<string> currentBranches)
    {
        int deletedCount = 0;
        foreach (string key in _states.Keys)
        {
            if (!currentBranches.Contains(key))
                deletedCount++;
        }
        
        if (deletedCount == 0)
            return default;
        
        (string Name, CommitSha LastSha, Timestamp LastTimestamp)[] array = 
            ArrayPool<(string, CommitSha, Timestamp)>.Shared.Rent(deletedCount);
        int index = 0;
        List<string>? toRemove = null;
        
        foreach (var kvp in _states)
        {
            if (!currentBranches.Contains(kvp.Key))
            {
                array[index++] = (kvp.Key, kvp.Value.Sha, kvp.Value.Timestamp);

                toRemove ??= new List<string>();
                toRemove.Add(kvp.Key);
            }
        }
        if (toRemove != null)
        {
            foreach (string name in toRemove)
                _states.Remove(name);
        }

        return new PooledDeletedBranches(array, deletedCount);
    }

    /// <summary>
    /// Internal state record for a tracked branch.
    /// </summary>
    private record struct BranchState(CommitSha Sha, Timestamp Timestamp);
}