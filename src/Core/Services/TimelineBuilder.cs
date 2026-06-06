using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
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

            var timeline = new Timeline(
                options.RepositoryId,
                EstimateInitialCapacity(commits, options));
            BuildCore(timeline, commits, options);
            
            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build timeline");
            return Result<Timeline>.Failure("Failed to build timeline", ex);
        }
    }

    public async Task<Result<Timeline>> BuildAsync(
        IAsyncEnumerable<CommitData> commits,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Building timeline from commit stream");

            var timeline = new Timeline(options.RepositoryId);
            var count = await BuildCoreAsync(timeline, commits, options, cancellationToken);

            logger.LogInformation("Built timeline from {Count} streamed commits", count);
            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build timeline from commit stream");
            return Result<Timeline>.Failure("Failed to build timeline", ex);
        }
    }

    private static void BuildCore(
        Timeline timeline,
        IReadOnlyList<CommitData> commits,
        TimelineBuilderOptions options)
    {
        if (!options.IncludeFileChanges && !options.IncludeBranchEvents && !options.IncludeMergeDetection)
        {
            for (var index = 0; index < commits.Count; index++)
                AddCommitEvent(timeline, commits[index]);

            return;
        }

        if (options.IncludeFileChanges && !options.IncludeBranchEvents && !options.IncludeMergeDetection)
        {
            for (var index = 0; index < commits.Count; index++)
            {
                var commit = commits[index];
                AddCommitEvent(timeline, commit);
                AddFileChangeEvents(timeline, commit);
            }

            return;
        }

        var branchTracker = options.IncludeBranchEvents
            ? new BranchTracker()
            : null;

        for (var index = 0; index < commits.Count; index++)
            AddCommitToTimeline(timeline, commits[index], options, branchTracker);
    }

    private async Task<int> BuildCoreAsync(
        Timeline timeline,
        IAsyncEnumerable<CommitData> commits,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken)
    {
        var count = 0;

        if (!options.IncludeFileChanges && !options.IncludeBranchEvents && !options.IncludeMergeDetection)
        {
            await foreach (var commit in commits.WithCancellation(cancellationToken))
            {
                AddCommitEvent(timeline, commit);
                count++;
                LogProgress(count);
            }

            return count;
        }

        if (options.IncludeFileChanges && !options.IncludeBranchEvents && !options.IncludeMergeDetection)
        {
            await foreach (var commit in commits.WithCancellation(cancellationToken))
            {
                AddCommitEvent(timeline, commit);
                AddFileChangeEvents(timeline, commit);
                count++;
                LogProgress(count);
            }

            return count;
        }

        var branchTracker = options.IncludeBranchEvents
            ? new BranchTracker()
            : null;

        await foreach (var commit in commits.WithCancellation(cancellationToken))
        {
            AddCommitToTimeline(timeline, commit, options, branchTracker);
            count++;
            LogProgress(count);
        }

        return count;
    }

    private void LogProgress(int count)
    {
        if (count % 50000 == 0)
            logger.LogInformation("Processed {Count} commits...", count);
    }

    private static void AddCommitToTimeline(
        Timeline timeline,
        CommitData commit,
        TimelineBuilderOptions options,
        BranchTracker? branchTracker)
    {
        AddCommitEvent(timeline, commit);
        if (options.IncludeFileChanges)
        {
            AddFileChangeEvents(timeline, commit);
        }
        if (options.IncludeBranchEvents && branchTracker is not null)
        {
            AddBranchEvents(timeline, commit, branchTracker);
        }
        if (options.IncludeMergeDetection && commit.IsMerge)
        {
            AddMergeEvent(timeline, commit);
        }
    }
    
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
    
    private static void AddFileChangeEvents(Timeline timeline, CommitData commit)
    {
        var timestamp = commit.Timestamp;
        var author = commit.Author;
        var sha = commit.Sha;

        foreach (var change in commit.FileChanges)
        {
            var evt = TraceEventFactory.FileChange(
                timestamp: timestamp,
                actor: author,
                path: change.Path,
                type: change.Kind,
                sha: sha,
                metadata: change.OldPath?.Value
            );

            timeline.AddEvent(evt);
        }
    }
    
    private static void AddBranchEvents(
        Timeline timeline,
        CommitData commit,
        BranchTracker tracker)
    {
        var timestamp = commit.Timestamp;
        var author = commit.Author;
        var sha = commit.Sha;
        var branches = commit.Branches;

        for (int index = 0; index < branches.Count; index++)
        {
            var branch = branches[index];
            bool isNew = tracker.TryUpdate(branch.Value, sha, timestamp);
            if (!isNew)
                continue;

            var evt = TraceEventFactory.Branch(
                timestamp: timestamp,
                actor: author,
                branch: branch,
                type: BranchEventType.BranchCreated,
                sha: sha,
                metadata: $"Created at {sha.Short}"
            );

            timeline.AddEvent(evt);
        }

        using var pooled = tracker.GetDeletedPooled(branches);
        var deleted = pooled.Span;

        for (int index = 0; index < deleted.Length; index++)
        {
            var (branchName, lastSha, lastTimestamp) = deleted[index];
            var branchNameResult = BranchName.Create(branchName);
            if (!branchNameResult.IsSuccess)
                continue;

            var evt = TraceEventFactory.Branch(
                timestamp: lastTimestamp,
                actor: author,
                branch: branchNameResult.Value,
                type: BranchEventType.BranchDeleted,
                sha: lastSha,
                metadata: $"Deleted (last: {lastSha.Short})"
            );

            timeline.AddEvent(evt);
        }
    }
    
    private static void AddMergeEvent(Timeline timeline, CommitData commit)
    {
        var parentShas = BuildParentSummary(commit.ParentShas);
        var metadata = string.Concat(commit.Message, " | Parents: ", parentShas);

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

    private static string BuildParentSummary(IReadOnlyList<CommitSha> parentShas)
    {
        if (parentShas.Count == 0)
            return string.Empty;

        if (parentShas.Count == 1)
            return parentShas[0].Short;

        var builder = new System.Text.StringBuilder(parentShas.Count * 10);

        for (var index = 0; index < parentShas.Count; index++)
        {
            if (index > 0)
                builder.Append(", ");

            builder.Append(parentShas[index].Short);
        }

        return builder.ToString();
    }

    private static int EstimateInitialCapacity(
        IReadOnlyList<CommitData> commits,
        TimelineBuilderOptions options)
    {
        long capacity = commits.Count;

        if (options.IncludeFileChanges)
        {
            for (int index = 0; index < commits.Count; index++)
                capacity += commits[index].FileChanges.Count;
        }

        if (options.IncludeBranchEvents)
        {
            for (int index = 0; index < commits.Count; index++)
                capacity += commits[index].Branches.Count * 2L;
        }

        if (options.IncludeMergeDetection)
        {
            for (int index = 0; index < commits.Count; index++)
            {
                if (commits[index].IsMerge)
                    capacity++;
            }
        }

        return capacity >= int.MaxValue
            ? int.MaxValue
            : (int)capacity;
    }
}
