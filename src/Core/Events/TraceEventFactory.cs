using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events;

/// <summary>
/// Factory responsible for constructing strongly-typed <see cref="TraceEvent"/> instances.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Encapsulates the construction logic of <see cref="TraceEvent"/>.</item>
/// <item>Ensures consistent creation of event structures across the system.</item>
/// <item>Provides domain-specific factory methods for commits, files, branches, pull requests, and merges.</item>
/// </list>
/// </remarks>
internal static class TraceEventFactory
{
    /// <summary>
    /// Creates a commit event representing a commit in the repository.
    /// </summary>
    /// <param name="timestamp">Timestamp of the commit.</param>
    /// <param name="actor">Actor responsible for the commit.</param>
    /// <param name="sha">Commit SHA identifier.</param>
    /// <param name="message">Optional commit message metadata.</param>
    /// <returns>A <see cref="TraceEvent"/> describing the commit.</returns>
    internal static TraceEvent Commit(
        Timestamp timestamp,
        ActorName actor,
        CommitSha sha,
        string? message = null)
    {
        var commitInfo = new CommitInfo(sha, FileChangeKind.Commit);
        var metadata = string.IsNullOrEmpty(message) ? (MetadataInfo?)null : new MetadataInfo(message);
        
        return new TraceEvent(
            new TraceEventCore(timestamp, actor, sha.Value),
            Commit: commitInfo,
            Metadata: metadata
        );
    }
    
    /// <summary>
    /// Creates a file change event representing a modification of a specific file in a commit.
    /// </summary>
    /// <param name="timestamp">Timestamp of the change.</param>
    /// <param name="actor">Actor responsible for the change.</param>
    /// <param name="path">Path of the affected file.</param>
    /// <param name="type">Type of commit-related change.</param>
    /// <param name="sha">Commit SHA associated with the change.</param>
    /// <param name="metadata">Optional additional metadata.</param>
    /// <returns>A <see cref="TraceEvent"/> describing the file change.</returns>
    internal static TraceEvent FileChange(
        Timestamp timestamp,
        ActorName actor,
        FilePath path,
        FileChangeKind type,
        CommitSha sha,
        string? metadata = null)
    {
        var commitInfo = new CommitInfo(sha, type);
        var metadatas = new MetadataInfo(metadata, path);

        return new TraceEvent(
            new TraceEventCore(timestamp, actor, path.Value),
            Commit: commitInfo,
            Metadata: metadatas
        );
    }

    /// <summary>
    /// Creates a branch event such as branch creation, deletion, or merge.
    /// </summary>
    /// <param name="timestamp">Timestamp of the branch event.</param>
    /// <param name="actor">Actor responsible for the action.</param>
    /// <param name="branch">Branch involved in the event.</param>
    /// <param name="type">Type of branch event.</param>
    /// <param name="sha">Optional commit associated with the branch event.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <returns>A <see cref="TraceEvent"/> describing the branch event.</returns>
    internal static TraceEvent Branch(
        Timestamp timestamp,
        ActorName actor,
        BranchName branch,
        BranchEventType type,
        CommitSha? sha = null,
        string? metadata = null)
    {
        var branchInfo = new BranchInfo(branch, type);
        var metadatas = metadata != null ? new MetadataInfo(metadata) : (MetadataInfo?)null;

        return new TraceEvent(
            new TraceEventCore(timestamp, actor, branch.Value),
            Commit: sha != null ? new CommitInfo(sha, FileChangeKind.Commit) : null,
            Branch: branchInfo,
            Metadata: metadatas
        );
    }

    /// <summary>
    /// Creates pull request event.
    /// </summary>
    /// <param name="timestamp">Timestamp of the pull request event.</param>
    /// <param name="actor">Actor responsible for the action.</param>
    /// <param name="number">Pull request number.</param>
    /// <param name="type">Type of pull request event.</param>
    /// <param name="branch">Optional branch associated with the pull request.</param>
    /// <param name="metadataStr">Optional metadata string.</param>
    /// <returns>A <see cref="TraceEvent"/> describing the pull request event.</returns>
    internal static TraceEvent PullRequest(
        Timestamp timestamp,
        ActorName actor,
        PullRequestNumber number,
        PullRequestEventType type,
        BranchName? branch = null,
        string? metadataStr = null)
    {
        var prInfo = new PullRequestInfo(number, type);
        var metadata = metadataStr != null ? new MetadataInfo(metadataStr) : (MetadataInfo?)null;

        return new TraceEvent(
            new TraceEventCore(timestamp, actor, branch?.Value ?? string.Empty),
            Branch: branch != null ? new BranchInfo(branch, BranchEventType.Merge) : null,
            PullRequest: prInfo,
            Metadata: metadata
        );
    }

    /// <summary>
    /// Creates a merge commit event representing a commit that merges into a target branch.
    /// </summary>
    /// <param name="timestamp">Timestamp of the merge commit.</param>
    /// <param name="actor">Actor responsible for the merge.</param>
    /// <param name="sha">Commit SHA of the merge commit.</param>
    /// <param name="target">Target branch receiving the merge.</param>
    /// <param name="message">Optional merge commit message.</param>
    /// <returns>A <see cref="TraceEvent"/> describing the merge event.</returns>
    internal static TraceEvent Merge(
        Timestamp timestamp,
        ActorName actor,
        CommitSha sha,
        BranchName target,
        string? message = null)
    {
        var commitInfo = new CommitInfo(sha, FileChangeKind.Commit);
        var branchInfo = new BranchInfo(target, BranchEventType.Merge);
        var metadata = message != null ? new MetadataInfo(message) : (MetadataInfo?)null;

        return new TraceEvent(
            new TraceEventCore(timestamp, actor, target.Value),
            Commit: commitInfo,
            Branch: branchInfo,
            Metadata: metadata
        );
    }
}