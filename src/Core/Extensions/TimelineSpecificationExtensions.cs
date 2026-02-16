using ChangeTrace.Core.Events;
using ChangeTrace.Core.Specifications;

namespace ChangeTrace.Core.Extensions;

/// <summary>
/// Provides extension methods for querying timelines using specifications.
/// Enables flexible, composable approach to filtering timeline events
/// without cluttering the core Timeline class with query logic.
/// </summary>
internal static class TimelineSpecificationExtensions
{
    /// <summary>
    /// Queries the timeline using a specification to filter events.
    /// </summary>
    /// <param name="timeline">The timeline to query.</param>
    /// <param name="spec">The specification that defines which events to include.</param>
    /// <returns>
    /// A filtered collection of <see cref="TraceEvent"/> that satisfy the given specification.
    /// </returns>
    internal static IEnumerable<TraceEvent> Query(
        this Timeline timeline,
        Specification<TraceEvent> spec)
    {
        return timeline.Events.Where(spec.IsSatisfiedBy);
    }
}