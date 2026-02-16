using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters file events to include only those within specified directory.
/// 
/// Comparison is case-insensitive and ensures directory path normalization.
/// </summary>
/// <param name="directory">Directory path to filter by</param>
internal sealed class InDirectorySpec(string directory) : Specification<TraceEvent>
{
    /// <summary>
    /// Normalized directory path (trailing slash removed).
    /// </summary>
    private readonly string _directory = directory.TrimEnd('/');

    /// <summary>
    /// Determines whether <see cref="TraceEvent"/> has a file path inside the configured directory.
    /// </summary>
    /// <param name="item">Trace event to evaluate</param>
    /// <returns>
    /// <c>true</c> if the event's file path is inside the directory; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.FilePath != null && item.FilePath.IsInDirectory(_directory);
}