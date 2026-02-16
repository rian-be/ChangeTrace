using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Temporal;

/// <summary>
/// Filters commits that occurred within a specific hour range (e.g., work hours, late night).
/// Handles ranges that wrap around midnight.
/// </summary>
internal sealed class CommitsBetweenHoursSpec(int startHour, int endHour) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the given event occurred within the specified hour range.
    /// </summary>
    /// <param name="item">The event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the commit occurred within the hour range; otherwise, <c>false</c>.
    /// Returns false for events that are not commits.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
    {
        if (item.CommitType == null)
            return false;

        var hour = DateTimeOffset.FromUnixTimeSeconds(item.Timestamp.UnixSeconds).UtcDateTime.Hour;
        var diff = (hour - startHour + 24) % 24;
        var range = (endHour - startHour + 24) % 24;

        return diff <= range;
    }
}