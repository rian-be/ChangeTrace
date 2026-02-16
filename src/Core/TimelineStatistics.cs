using ChangeTrace.Core.Models;

namespace ChangeTrace.Core;

/// <summary>
/// Aggregated statistics for <see cref="Timeline"/>.
/// Provides counts of events, commits, branches, PRs, and actors, as well as the time span.
/// </summary>
/// <param name="TotalEvents">Total number of events in the timeline.</param>
/// <param name="CommitCount">Number of commit events.</param>
/// <param name="BranchCount">Number of branch events.</param>
/// <param name="PullRequestCount">Number of pull request events.</param>
/// <param name="MergeCommitCount">Number of merge commits.</param>
/// <param name="UniqueActors">Number of unique actors involved in events.</param>
/// <param name="TimeSpanSeconds">Total time span of the timeline in seconds.</param>
/// <param name="StartTime">Start time of the timeline.</param>
/// <param name="EndTime">End time of the timeline.</param>
internal sealed record TimelineStatistics(
    int TotalEvents,
    int CommitCount,
    int BranchCount,
    int PullRequestCount,
    int MergeCommitCount,
    int UniqueActors,
    long TimeSpanSeconds,
    Timestamp StartTime,
    Timestamp EndTime)
{
    /// <summary>
    /// Empty statistics with zero counts and current timestamps.
    /// </summary>
    public static TimelineStatistics Empty => new(
        0, 0, 0, 0, 0, 0, 0,
        Timestamp.Now,
        Timestamp.Now
    );

    /// <summary>
    /// Human-readable summary of the statistics.
    /// </summary>
    public override string ToString() =>
        $"Events: {TotalEvents}, Commits: {CommitCount}, PRs: {PullRequestCount}, " +
        $"Actors: {UniqueActors}, Span: {TimeSpanSeconds}s";
}