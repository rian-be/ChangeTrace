using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events;

/// <summary>
/// Factory for creating strongly-typed timeline events.
/// Provides clean, fluent API for constructing different types of trace events
/// while hiding the complexity of the underlying constructor.
/// </summary>
internal static class TraceEventFactory
{
    /// <summary>
    /// Creates commit event.
    /// </summary>
    /// <param name="timestamp">When the commit occurred.</param>
    /// <param name="actor">Who made the commit.</param>
    /// <param name="sha">The commit's unique SHA identifier.</param>
    /// <param name="message">Optional commit message.</param>
    /// <returns>A trace event representing a standard commit.</returns>
    internal static TraceEvent Commit(
        Timestamp timestamp,
        ActorName actor,
        CommitSha sha,
        string? message = null)
        => new(
            timestamp,
            actor,
            sha.Value,
            message,
            sha,
            null,
            null,
            null,
            CommitEventType.Commit,
            null,
            null
        );

    /// <summary>
    /// Creates file change event.
    /// Represents a specific file modification within a commit.
    /// </summary>
    /// <param name="timestamp">When the file change occurred.</param>
    /// <param name="actor">Who made the change.</param>
    /// <param name="path">Path to the changed file.</param>
    /// <param name="type">Type of change (Added, Modified, Deleted, Renamed).</param>
    /// <param name="sha">The commit SHA containing this change.</param>
    /// <param name="metadata">Optional additional information about the change.</param>
    /// <returns>A trace event representing a file-level change.</returns>
    internal static TraceEvent FileChange(
        Timestamp timestamp,
        ActorName actor,
        FilePath path,
        CommitEventType type,
        CommitSha sha,
        string? metadata = null)
        => new(
            timestamp,
            actor,
            path.Value,
            metadata,
            sha,
            null,
            null,
            path,
            type,
            null,
            null
        );

    /// <summary>
    /// Creates branch event.
    /// Represents branch operations like creation, deletion, or checkout.
    /// </summary>
    /// <param name="timestamp">When the branch operation occurred.</param>
    /// <param name="actor">Who performed the branch operation.</param>
    /// <param name="branch">The branch name.</param>
    /// <param name="type">Type of branch operation (Create, Delete, Checkout).</param>
    /// <param name="sha">Optional commit SHA associated with the branch operation.</param>
    /// <param name="metadata">Optional additional information about the branch operation.</param>
    /// <returns>A trace event representing a branch operation.</returns>
    internal static TraceEvent Branch(
        Timestamp timestamp,
        ActorName actor,
        BranchName branch,
        BranchEventType type,
        CommitSha? sha = null,
        string? metadata = null)
        => new(
            timestamp,
            actor,
            branch.Value,
            metadata,
            sha,
            branch,
            null,
            null,
            null,
            type,
            null
        );

    /// <summary>
    /// Creates merge commit event.
    /// Specialized commit event representing a merge operation.
    /// </summary>
    /// <param name="timestamp">When the merge occurred.</param>
    /// <param name="actor">Who performed the merge.</param>
    /// <param name="sha">The merge commit SHA.</param>
    /// <param name="target">Optional target branch that received the merge.</param>
    /// <param name="message">Optional merge commit message.</param>
    /// <returns>A trace event representing a merge commit.</returns>
    internal static TraceEvent Merge(
        Timestamp timestamp,
        ActorName actor,
        CommitSha sha,
        BranchName? target = null,
        string? message = null)
        => new(
            timestamp,
            actor,
            sha.Value,
            message,
            sha,
            target,
            null,
            null,
            CommitEventType.Commit,
            BranchEventType.Merge,
            null
        );
}