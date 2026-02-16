using ChangeTrace.Core;

namespace ChangeTrace.GIt.Enrichers;

/// <summary>
/// Represents the result of enriching a <see cref="Timeline"/> with pull request data
/// from an external platform (e.g., GitHub).
/// </summary>
/// <param name="TotalPullRequests">
/// Total number of pull requests fetched from the platform.
/// </param>
/// <param name="MatchedCount">
/// Number of pull requests successfully associated with commits or branches in the timeline.
/// </param>
/// <param name="UnmatchedCount">
/// Number of pull requests that could not be matched to any existing timeline events.
/// </param>
public sealed record EnrichmentResult(
    int TotalPullRequests,
    int MatchedCount,
    int UnmatchedCount
);