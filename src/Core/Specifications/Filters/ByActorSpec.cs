using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters events performed by specific actor.
/// 
/// Matches only events whose actor exactly equals the provided identity.
/// Comparison is value-based and assumes normalized actor names.
/// </summary>
/// <param name="actor">Actor that must have performed the event</param>
internal sealed class ByActorSpec(ActorName actor) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the event was performed by the specified actor.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the event actor matches the configured actor;
    /// otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item) 
        => item.Actor == actor;
}