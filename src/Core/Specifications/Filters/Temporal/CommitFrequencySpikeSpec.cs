using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Temporal;

/// <summary>
/// Detects commits that occur in bursts above a threshold within one hour.
/// Each hour is tracked independently, and any hour exceeding the threshold
/// marks commits in that hour as high-frequency.
/// </summary>
/// <param name="commitsPerHourThreshold">Minimum commits per hour to consider a burst.</param>
internal sealed class CommitFrequencySpikeSpec(int commitsPerHourThreshold) : Specification<TraceEvent>
{
    /// <summary>
    /// Maps each hour (Unix timestamp / 3600) to the number of commits in that hour.
    /// </summary>
    private readonly Dictionary<long, int> _hourCounts = new();

    /// <summary>
    /// Determines whether the specified <paramref name="item"/> is part of a high-frequency burst.
    /// </summary>
    /// <param name="item">The trace event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the commit is part of a burst exceeding <c>commitsPerHourThreshold</c>;
    /// otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
    {
        if (item.CommitType == null)
            return false;

        long hourKey = item.Timestamp.UnixSeconds / 3600;

        if (!_hourCounts.TryAdd(hourKey, 1))
            _hourCounts[hourKey]++;
        
        return _hourCounts[hourKey] >= commitsPerHourThreshold;
    }
}