using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents a validated time range with start and end timestamps.
/// 
/// Provides duration, containment, and overlap checks for filtering events.
/// </summary>
internal readonly struct TimeRange
{
    private Timestamp Start { get; }
    private Timestamp End { get; }
    
    public long DurationSeconds => End.UnixSeconds - Start.UnixSeconds;

    private TimeRange(Timestamp start, Timestamp end)
    {
        Start = start;
        End = end;
    }
    
    /// <summary>
    /// Creates <see cref="TimeRange"/> from start and end timestamps.
    /// Returns failure if start is after end.
    /// </summary>
    /// <param name="start">Start timestamp</param>
    /// <param name="end">End timestamp</param>
    /// <returns>Result containing a validated <see cref="TimeRange"/> or an error</returns>
    public static Result<TimeRange> Create(Timestamp start, Timestamp end)
    {
        return start > end
            ? Result<TimeRange>.Failure("Start must be before end")
            : Result<TimeRange>.Success(new TimeRange(start, end));
    }

    /// <summary>
    /// Determines if timestamp is within the range (inclusive).
    /// </summary>
    /// <param name="timestamp">Timestamp to check</param>
    public bool Contains(Timestamp timestamp) => timestamp >= Start && timestamp <= End;

    /// <summary>
    /// Determines if this range overlaps with another range.
    /// </summary>
    /// <param name="other">Other time range</param>
    public bool Overlaps(TimeRange other) => Start <= other.End && End >= other.Start;

    public override string ToString() => $"{Start} - {End}";
}