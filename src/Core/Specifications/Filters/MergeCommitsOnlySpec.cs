using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters events to include only merge commits.
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
        => item.Branch is { Type: BranchEventType.Merge };
}