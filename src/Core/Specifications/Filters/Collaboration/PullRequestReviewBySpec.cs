using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Collaboration;

/// <summary>
/// Matches pull requests that have been reviewed by specific actor.
/// </summary>
internal sealed class PullRequestReviewBySpec(ActorName reviewer) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the trace event represents pull request
    /// reviewed by the specified actor.
    /// </summary>
    /// <param name="item">The trace event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the pull request has the specified reviewer; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.HasPullRequest()
           && item.Reviewers is not null
           && item.Reviewers.Contains(reviewer);
}