using System.Text;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;

namespace ChangeTrace.Core.Services;

/// <summary>
/// Shared event emission logic used by both in-memory timeline building and stream-based persistence.
/// </summary>
internal static class TimelineEventEmitter
{
    /// <summary>
    /// Emits a single commit event into the provided sink.
    /// </summary>
    internal static void EmitCommit(
        CommitData commit,
        Action<TraceEvent> sink)
    {
        sink(TraceEventFactory.Commit(
            timestamp: commit.Timestamp,
            actor: commit.Author,
            sha: commit.Sha,
            message: commit.Message));
    }

    /// <summary>
    /// Emits file change events for a commit into the provided sink.
    /// </summary>
    internal static void EmitFileChanges(
        CommitData commit,
        Action<TraceEvent> sink)
    {
        var timestamp = commit.Timestamp;
        var author = commit.Author;
        var sha = commit.Sha;

        foreach (var change in commit.FileChanges)
        {
            sink(TraceEventFactory.FileChange(
                timestamp: timestamp,
                actor: author,
                path: change.Path,
                type: change.Kind,
                sha: sha,
                metadata: change.OldPath?.Value));
        }
    }

    /// <summary>
    /// Emits branch creation and deletion events for a commit into the provided sink.
    /// </summary>
    internal static void EmitBranchEvents(
        CommitData commit,
        BranchTracker tracker,
        Action<TraceEvent> sink)
    {
        var timestamp = commit.Timestamp;
        var author = commit.Author;
        var sha = commit.Sha;
        var branches = commit.Branches;

        foreach (var branch in branches)
        {
            bool isNew = tracker.TryUpdate(branch.Value, sha, timestamp);
            if (!isNew)
                continue;

            sink(TraceEventFactory.Branch(
                timestamp: timestamp,
                actor: author,
                branch: branch,
                type: BranchEventType.BranchCreated,
                sha: sha,
                metadata: $"Created at {sha.Short}"));
        }

        using var pooled = tracker.GetDeletedPooled(branches);
        var deleted = pooled.Span;

        foreach (var (branchName, lastSha, lastTimestamp) in deleted)
        {
            var branchNameResult = BranchName.Create(branchName);
            if (!branchNameResult.IsSuccess)
                continue;

            sink(TraceEventFactory.Branch(
                timestamp: lastTimestamp,
                actor: author,
                branch: branchNameResult.Value,
                type: BranchEventType.BranchDeleted,
                sha: lastSha,
                metadata: $"Deleted (last: {lastSha.Short})"));
        }
    }

    /// <summary>
    /// Emits a merge event for the specified commit.
    /// </summary>
    internal static void EmitMergeEvent(
        CommitData commit,
        Action<TraceEvent> sink)
    {
        var parentShas = BuildParentSummary(commit.ParentShas);
        var metadata = string.Concat(commit.Message, " | Parents: ", parentShas);

        var branch = commit.Branches.FirstOrDefault()
                     ?? BranchName.Create("unknown").Value;

        sink(TraceEventFactory.Merge(
            timestamp: commit.Timestamp,
            actor: commit.Author,
            sha: commit.Sha,
            target: branch,
            message: metadata));
    }

    /// <summary>
    /// Emits all timeline events requested by the provided options.
    /// </summary>
    internal static void EmitCommitEvents(
        CommitData commit,
        TimelineBuilderOptions options,
        BranchTracker? branchTracker,
        Action<TraceEvent> sink)
    {
        EmitCommit(commit, sink);

        if (options.IncludeFileChanges)
            EmitFileChanges(commit, sink);

        if (options.IncludeBranchEvents && branchTracker is not null)
            EmitBranchEvents(commit, branchTracker, sink);

        if (options.IncludeMergeDetection && commit.IsMerge)
            EmitMergeEvent(commit, sink);
    }

    /// <summary>
    /// Estimates the number of events that will be emitted for the supplied commits.
    /// </summary>
    internal static int EstimateCapacity(
        IReadOnlyList<CommitData> commits,
        TimelineBuilderOptions options)
    {
        long capacity = commits.Count;

        if (options.IncludeFileChanges)
        {
            foreach (var t in commits)
                capacity += t.FileChanges.Count;
        }

        if (options.IncludeBranchEvents)
        {
            foreach (var t in commits)
                capacity += t.Branches.Count * 2L;
        }

        if (options.IncludeMergeDetection)
        {
            foreach (var t in commits)
            {
                if (t.IsMerge)
                    capacity++;
            }
        }

        return capacity >= int.MaxValue
            ? int.MaxValue
            : (int)capacity;
    }

    private static string BuildParentSummary(IReadOnlyList<CommitSha> parentShas)
    {
        if (parentShas.Count == 0)
            return string.Empty;

        if (parentShas.Count == 1)
            return parentShas[0].Short;

        var builder = new StringBuilder(parentShas.Count * 10);

        for (var index = 0; index < parentShas.Count; index++)
        {
            if (index > 0)
                builder.Append(", ");

            builder.Append(parentShas[index].Short);
        }

        return builder.ToString();
    }
}
