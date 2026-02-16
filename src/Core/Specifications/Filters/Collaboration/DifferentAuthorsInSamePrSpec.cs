using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Collaboration;

/// <summary>
/// Matches Pull Requests where multiple distinct authors contributed.
/// </summary>
internal sealed class DifferentAuthorsInSamePrSpec : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the event represents PR with contributions
    /// from more than one distinct author.
    /// </summary>
    /// <param name="item">The trace event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the event has PR data and more than one contributor;
    /// otherwise, <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.HasPullRequest() && item.Contributors is { Count: > 1 } 
                                 && item.Contributors.Distinct().Count() > 1;
}