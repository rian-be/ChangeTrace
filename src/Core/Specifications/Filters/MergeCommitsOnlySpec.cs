using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters events to include only merge commits.
/// 
/// Matches events where <see cref="TraceEvent.IsMergeCommit"/> returns true.
/// </summary>
internal sealed class MergeCommitsOnlySpec : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether a <see cref="TraceEvent"/> represents a merge commit.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> if the event is a merge commit; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item) 
        => item.IsMergeCommit();
}