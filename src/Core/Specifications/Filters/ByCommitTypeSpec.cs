using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters events by commit event classification.
/// 
/// Matches only events whose commit type equals the specified value.
/// Non-commit events are implicitly excluded.
/// </summary>
/// <param name="commitType">Commit event type to match</param>
internal sealed class ByCommitTypeSpec(CommitEventType commitType) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the event has the specified commit type.
    /// </summary>
    /// <param name="item">Event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the event commit type matches the configured value;
    /// otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.CommitType == commitType;
}