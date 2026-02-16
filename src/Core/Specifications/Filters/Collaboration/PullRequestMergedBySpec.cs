using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Collaboration;

/// <summary>
/// Matches pull requests merged by specific actor.
/// </summary>
internal sealed class PullRequestMergedBySpec(ActorName merger) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the trace event is merge commit
    /// performed by the specified actor.
    /// </summary>
    /// <param name="item">The trace event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the event is a merge commit by the specified actor; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.IsMergeCommit()
           && item.MergedBy is not null
           && item.MergedBy == merger;
}