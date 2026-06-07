using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.Core.Services;

/// <summary>
/// Builds timeline from commit data.
/// Handles commit, file change, branch and merge event emission.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TimelineBuilder(ILogger<TimelineBuilder> logger) : ITimelineBuilder
{
    /// <summary>
    /// Builds timeline from an asynchronous commit stream.
    /// </summary>
    public async Task<Result<Timeline>> Build(
        IAsyncEnumerable<CommitData> commits,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Building timeline from commit stream");

            var timeline = new Timeline(options.RepositoryId);
            var count = await BuildCore(timeline, commits, options, cancellationToken);

            logger.LogInformation("Built timeline from {Count} streamed commits", count);
            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to build timeline from commit stream");
            return Result<Timeline>.Failure("Failed to build timeline", ex);
        }
    }

    private async Task<int> BuildCore(
        Timeline timeline,
        IAsyncEnumerable<CommitData> commits,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken)
    {
        var count = 0;

        if (IsCommitOnly(options))
        {
            await foreach (var commit in commits.WithCancellation(cancellationToken))
            {
                TimelineEventEmitter.EmitCommit(commit, timeline.AddEvent);
                count++;
            }

            return count;
        }

        if (IsCommitAndFileChangesOnly(options))
        {
            await foreach (var commit in commits.WithCancellation(cancellationToken))
            {
                TimelineEventEmitter.EmitCommit(commit, timeline.AddEvent);
                TimelineEventEmitter.EmitFileChanges(commit, timeline.AddEvent);
                count++;
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
        }

        return count;
    }

    private static bool IsCommitOnly(TimelineBuilderOptions options)
        => !options.IncludeFileChanges && !options.IncludeBranchEvents && !options.IncludeMergeDetection;

    private static bool IsCommitAndFileChangesOnly(TimelineBuilderOptions options)
        => options.IncludeFileChanges && !options.IncludeBranchEvents && !options.IncludeMergeDetection;

    private static void AddCommitToTimeline(
        Timeline timeline,
        CommitData commit,
        TimelineBuilderOptions options,
        BranchTracker? branchTracker)
    {
        TimelineEventEmitter.EmitCommitEvents(
            commit,
            options,
            branchTracker,
            timeline.AddEvent);
    }
}
