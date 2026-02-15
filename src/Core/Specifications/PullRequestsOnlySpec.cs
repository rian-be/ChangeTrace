using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Filters events to include only pull request events.
/// 
/// Matches events that have a defined <see cref="TraceEvent.PrType"/>.
/// Non pull request events are excluded.
/// </summary>
internal sealed class PullRequestsOnlySpec : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether <see cref="TraceEvent"/> represents pull request event.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the event has a pull request type; otherwise <c>false</c>.
    /// </returns>
    public override bool IsSatisfiedBy(TraceEvent item) 
        => item.PrType.HasValue;
}