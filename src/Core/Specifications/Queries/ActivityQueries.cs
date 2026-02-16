using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters;

namespace ChangeTrace.Core.Specifications.Queries;

/// <summary>
/// Activity queries providing reusable specifications for <see cref="TraceEvent"/> instances.
/// These queries allow filtering events based on temporal criteria.
/// </summary>
/// <remarks>
/// Use this class to compose specifications for filtering events by time-related criteria:
/// - Events occurring within the last N hours
/// </remarks>
internal static class ActivityQueries
{
    /// <summary>
    /// Creates specification that filters events occurring within the last <paramref name="hours"/> hours.
    /// </summary>
    /// <param name="hours">The number of hours in the past to include events from.</param>
    /// <returns>
    /// A specification matching events that occurred between <paramref name="hours"/> ago and the current time.
    /// Combines <see cref="InTimeRangeSpec"/> with <see cref="Timestamp"/> for the range calculation.
    /// </returns>
    public static Specification<TraceEvent> Recent(int hours)
    {
        var now = Timestamp.Now;
        var cutoff = Timestamp.Create(now.UnixSeconds - hours * 3600).Value;
        return new InTimeRangeSpec(cutoff, now);
    }
}