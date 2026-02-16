using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Ownership;

/// <summary>
/// Filter that matches only commit events for a specific file.
/// </summary>
internal sealed class ByFileSpec(FilePath filePath) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether the specified <see cref="TraceEvent"/> matches the file filter.
    /// </summary>
    /// <param name="item">The trace event to evaluate.</param>
    /// <returns><c>true</c> if the event is a commit for the specified file; otherwise, <c>false</c>.</returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
    {
        return item is { CommitType: not null, FilePath: not null } &&
               item.FilePath.Equals(filePath);
    }
}