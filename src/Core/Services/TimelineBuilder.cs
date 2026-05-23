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

            var timeline = new Timeline(options.RepositoryId);
            var branchTracker = new BranchTracker();

            foreach (var commit in commits)
            {
                AddCommitEvent(timeline, commit);
                if (options.IncludeFileChanges)
                {
                    AddFileChangeEvents(timeline, commit);
                }
                if (options.IncludeBranchEvents)
                {
                    AddBranchEvents(timeline, commit, branchTracker);
                }
                if (options.IncludeMergeDetection && commit.IsMerge)
                {
                    AddMergeEvent(timeline, commit);
                }
            }
            
            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build timeline");
            return Result<Timeline>.Failure("Failed to build timeline", ex);
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
        foreach (var change in commit.FileChanges)
        {
            var evt = TraceEventFactory.FileChange(
                timestamp: commit.Timestamp,
                actor: commit.Author,
                path: change.Path,
                type: change.Kind,  
                sha: commit.Sha,
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
        var currentBranches = commit.Branches.Select(b => b.Value).ToHashSet();

        foreach (var branch in commit.Branches)
        {
            bool isNew = tracker.TryUpdate(branch.Value, commit.Sha, commit.Timestamp);
            if (!isNew) continue;
            
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

        using var pooled = tracker.GetDeletedPooled(currentBranches);
        var deleted = pooled.Span;

        foreach (var (branchName, lastSha, lastTimestamp) in deleted)
        {
            var branchNameResult = BranchName.Create(branchName);
            if (!branchNameResult.IsSuccess)
                continue;


            var evt = TraceEventFactory.Branch(
                timestamp: lastTimestamp,
                actor: commit.Author,
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
}