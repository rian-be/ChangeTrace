using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Contribution;

/// <summary>
/// Matches events where an author returns to contribute after a period of inactivity.
/// Useful to detect reactivated contributors.
/// </summary>
internal sealed class ReturningAfterInactivitySpec(int inactivityHours = 168) : Specification<TraceEvent> // inactivityHours default 7 days 
{
    private readonly IDictionary<ActorName, Timestamp> _lastContribution = new Dictionary<ActorName, Timestamp>();
    
    /// <summary>
    /// Checks if a given TraceEvent is from an author returning after inactivity.
    /// </summary>
    /// <param name="item">The event to evaluate</param>
    /// <returns><c>true</c> if the author was inactive for more than the threshold; otherwise <c>false</c>.</returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
    {
        if (_lastContribution.TryGetValue(item.Actor, out var last))
        {
            var hoursSinceLast = (item.Timestamp.UnixSeconds - last.UnixSeconds) / 3600.0;
            _lastContribution[item.Actor] = item.Timestamp;
            return hoursSinceLast >= inactivityHours;
        }

        _lastContribution[item.Actor] = item.Timestamp;
        return false;
    }
}