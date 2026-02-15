using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications;

/// <summary>
/// Filters events to include only those within specific time range.
/// 
/// Inclusive of the start and end timestamps.
/// </summary>
/// <param name="start">Start of the time range</param>
/// <param name="end">End of the time range</param>
internal sealed class InTimeRangeSpec(Timestamp start, Timestamp end) : Specification<TraceEvent>
{
    /// <summary>
    /// Determines whether a <see cref="TraceEvent"/> occurs within the configured time range.
    /// </summary>
    /// <param name="item">Trace event to evaluate</param>
    /// <returns>
    /// <c>true</c> if the event's timestamp is between <paramref>
    ///         <name>start</name>
    ///     </paramref>
    ///     and <paramref>
    ///         <name>end</name>
    ///     </paramref>
    ///     ; otherwise <c>false</c>.
    /// </returns>
    public override bool IsSatisfiedBy(TraceEvent item)
        => item.Timestamp >= start && item.Timestamp <= end;
}