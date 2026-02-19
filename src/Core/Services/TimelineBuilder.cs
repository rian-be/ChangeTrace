using ChangeTrace.Configuration;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.Core.Services;

/// <summary>
/// Builds a timeline from raw commit data.
/// Transforms low level Git commit information into a structured timeline of events
/// (commits, file changes, branch operations, merges) based on configuration options.
/// Focuses on performance and clarity with minimal allocations.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TimelineBuilder(ILogger<TimelineBuilder> logger) : ITimelineBuilder
{
    /// <summary>
    /// Builds a timeline from a collection of commits.
    /// </summary>
    /// <param name="commits">The raw commit data to process.</param>
    /// <param name="options">Configuration options controlling which events are generated.</param>
    /// <returns>
    /// A <see cref="Result{Timeline}"/> containing the constructed timeline with all requested events,
    /// or failure with error details if building fails.
    /// </returns>
    public Result<Timeline> Build(
        IReadOnlyList<CommitData> commits,
        TimelineBuilderOptions options)
    {
        try
        {
            logger.LogInformation("Building timeline from {Count} commits", commits.Count);

            var timeline = new Timeline(options.Name, options.RepositoryId);
            var branchTracker = new BranchTracker();

            foreach (var commit in commits)
            {
                // 1. Main commit event
                AddCommitEvent(timeline, commit);

                // 2. File changes
                if (options.IncludeFileChanges)
                {
                    AddFileChangeEvents(timeline, commit);
                }

                // 3. Branch events
                if (options.IncludeBranchEvents)
                {
                    AddBranchEvents(timeline, commit, branchTracker);
                }

                // 4. Merge detection
                if (options.IncludeMergeDetection && commit.IsMerge)
                {
                    AddMergeEvent(timeline, commit);
                }
            }

            logger.LogInformation("Timeline built: {Stats}", timeline.GetStatistics());
            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build timeline");
            return Result<Timeline>.Failure("Failed to build timeline", ex);
        }
    }

    /// <summary>
    /// Adds main commit event to the timeline.
    /// </summary>
    private static void AddCommitEvent(Timeline timeline, CommitData commit)
    {
        var evt = TraceEventFactory.Commit(
            timestamp: commit.Timestamp,
            actor: commit.Author,
            sha: commit.Sha,
            message: commit.Message
        );

        timeline.AddEvent(evt);
    }

    /// <summary>
    /// Adds individual file change events for each modified file in the commit.
    /// Called only when IncludeFileChanges is true.
    /// </summary>
    private static void AddFileChangeEvents(Timeline timeline, CommitData commit)
    {
        foreach (var change in commit.FileChanges)
        {
            var changeType = MapChangeKind(change.Kind);

            var evt = TraceEventFactory.FileChange(
                timestamp: commit.Timestamp,
                actor: commit.Author,
                path: change.Path,
                type: changeType,
                sha: commit.Sha,
                metadata: change.OldPath?.Value
            );

            timeline.AddEvent(evt);
        }
    }
    
    /// <summary>
    /// Adds branch related events (creation, updates, deletion) to the timeline.
    /// Uses branch tracker to detect state changes across commits.
    /// Called only when IncludeBranchEvents is true.
    /// </summary>
    private static void AddBranchEvents(
        Timeline timeline,
        CommitData commit,
        BranchTracker tracker)
    {
        var currentBranches = commit.Branches.Select(b => b.Value).ToHashSet();

        foreach (var branch in commit.Branches)
        {
            if (tracker.IsNew(branch.Value))
            {
                var evt = TraceEventFactory.Branch(
                    timestamp: commit.Timestamp,
                    actor: commit.Author,
                    branch: branch,
                    type: BranchEventType.BranchCreated,
                    sha: commit.Sha,
                    metadata: $"Created at {commit.Sha.Short}"
                );

                timeline.AddEvent(evt);
            }

            tracker.Update(branch.Value, commit.Sha, commit.Timestamp);
        }

        var deleted = tracker.GetDeleted(currentBranches);

        foreach (var (branchName, lastSha) in deleted)
        {
            var branchNameResult = BranchName.Create(branchName);
            if (!branchNameResult.IsSuccess)
                continue;

            var evt = TraceEventFactory.Branch(
                timestamp: commit.Timestamp,
                actor: commit.Author,
                branch: branchNameResult.Value,
                type: BranchEventType.BranchDeleted,
                sha: lastSha,
                metadata: $"Deleted (last: {lastSha.Short})"
            );

            timeline.AddEvent(evt);
        }
    }
    
    /// <summary>
    /// Adds merge commit event to the timeline.
    /// Called only when IncludeMergeDetection is true and commit is a merge.
    /// </summary>
    private static void AddMergeEvent(Timeline timeline, CommitData commit)
    {
        var parentShas = string.Join(", ", commit.ParentShas.Select(s => s.Short));
        var metadata = $"{commit.Message} | Parents: {parentShas}";

        var branch = commit.Branches.FirstOrDefault()
                     ?? BranchName.Create("unknown").Value;

        var evt = TraceEventFactory.Merge(
            timestamp: commit.Timestamp,
            actor: commit.Author,
            sha: commit.Sha,
            target: branch,
            message: metadata
        );

        timeline.AddEvent(evt);
    }

    /// <summary>
    /// Maps a file change kind from Git to the corresponding commit event type.
    /// </summary>
    private static CommitEventType MapChangeKind(FileChangeKind kind) => kind switch
    {
        FileChangeKind.Added => CommitEventType.FileAdded,
        FileChangeKind.Modified => CommitEventType.FileModified,
        FileChangeKind.Deleted => CommitEventType.FileDeleted,
        FileChangeKind.Renamed => CommitEventType.FileRenamed,
        _ => CommitEventType.FileModified
    };
}