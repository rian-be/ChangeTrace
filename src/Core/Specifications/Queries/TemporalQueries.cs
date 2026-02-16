using ChangeTrace.Core.Models;
using ChangeTrace.Core.Specifications.Filters.Temporal;

namespace ChangeTrace.Core.Specifications.Queries;

/// <summary>
/// Temporal queries providing reusable specifications for <see cref="TraceEvent"/> instances.
/// These queries allow filtering events based on time-related patterns, such as work hours, late-night activity, or commit bursts.
/// </summary>
/// <remarks>
/// Use this class to compose specifications for filtering events by temporal criteria:
/// - Commits during standard work hours
/// - Commits during late-night hours
/// - Spikes in commit frequency (release bursts)
/// </remarks>
internal static class TemporalQueries
{
    /// <summary>
    /// Creates specification for commits occurring during standard work hours (8:00–17:00 UTC).
    /// </summary>
    /// <returns>A specification matching commits made within standard work hours.</returns>
    public static Specification<TraceEvent> WorkHours()
        => new CommitsBetweenHoursSpec(8, 17);

    /// <summary>
    /// Creates specification for commits occurring during late-night hours (0:00–5:00 UTC).
    /// </summary>
    /// <returns>A specification matching commits made during late-night hours.</returns>
    public static Specification<TraceEvent> LateNight()
        => new CommitsBetweenHoursSpec(0, 5);

    /// <summary>
    /// Creates specification for detecting bursts of commit activity.
    /// </summary>
    /// <param name="commitsPerHour">The minimum number of commits per hour to consider a burst.</param>
    /// <returns>
    /// A specification matching periods where commit frequency exceeds the given <paramref name="commitsPerHour"/>.
    /// </returns>
    public static Specification<TraceEvent> ReleaseBursts(int commitsPerHour)
        => new CommitFrequencySpikeSpec(commitsPerHour);
}