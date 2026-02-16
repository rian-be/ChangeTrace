using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Collaboration;

/// <summary>
/// Matches files that have been modified by more than one author
/// within a short time window.
/// </summary>
internal sealed class SameFileDifferentAuthorsShortWindowSpec(int timeWindowMinutes = 60) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the trace event represents a file
    /// modified by multiple authors within the configured time window.
    /// </summary>
    /// <param name="item">The trace event to evaluate.</param>
    /// <returns>
    /// <c>true</c> if multiple authors modified the same file recently; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
    {
        if (item.FilePath == null || item.Contributors == null || item.LastModified == null)
            return false;

        var recentAuthors = item.Contributors
            .Where(c => item.LastModified.TryGetValue(c, out var lastTime) &&
                        ((item.Timestamp.UnixSeconds - lastTime.UnixSeconds) / 60.0) <= timeWindowMinutes)
            .Distinct()
            .ToList();

        return recentAuthors.Count > 1;
    }
}