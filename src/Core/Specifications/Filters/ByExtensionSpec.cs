using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters;

/// <summary>
/// Filters file events by file extension.
/// 
/// Matches only events whose file path extension equals the specified value.
/// Comparison is case-insensitive, and a leading dot is optional in the input.
/// </summary>
/// <param name="extension">File extension to match (e.g., ".cs" or "cs")</param>
internal sealed class ByExtensionSpec(string extension) : Specification<TraceEvent>
{
    /// <summary>
    /// Normalized extension with leading dot for comparison.
    /// </summary>
    private readonly string _extension = extension.StartsWith('.') ? extension : $".{extension}";

    /// <summary>
    /// Determines whether the event's file path has the configured extension.
    /// </summary>
    /// <param name="item">Trace event to evaluate</param>
    /// <returns>
    /// <c>true</c> when the file path has the specified extension; otherwise <c>false</c>.
    /// </returns>
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.FilePath != null &&
           item.FilePath.Extension.Equals(_extension, StringComparison.OrdinalIgnoreCase);
}