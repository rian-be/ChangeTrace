using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Filters events that are associated with Git branch.
/// 
/// Matches only events that carry branch metadata,
/// excluding commits, tags, PRs and other repository activities
/// not tied to a branch reference.
/// </summary>
internal sealed class BranchesOnlySpec : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the event represents a branch-related action.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the event contains branch information;
    /// otherwise <c>false</c>.
    /// </returns>
    public override bool IsSatisfiedBy(TraceEvent item) 
        => item.BranchType.HasValue;
}