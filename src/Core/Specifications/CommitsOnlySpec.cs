using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Filters events to include only commit-related events.
/// 
/// Matches events that have a defined <see cref="TraceEvent.CommitType"/>.
/// Non-commit events are excluded.
/// </summary>
internal sealed class CommitsOnlySpec : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether <see cref="TraceEvent"/> represents a commit event.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the event has a commit type; otherwise <c>false</c>.
    /// </returns>
    public override bool IsSatisfiedBy(TraceEvent item) 
        => item.CommitType.HasValue;
}