using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters events associated with specific branch.
/// 
/// Matches only events that reference branch and whose branch name
/// equals the provided value.
/// </summary>
/// <param name="branchName">Branch that must be associated with the event</param>
internal sealed class ByBranchSpec(BranchName branchName) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the event belongs to the specified branch.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the event references the given branch;
    /// otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.BranchName != null && item.BranchName == branchName;
}