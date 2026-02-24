using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents a Unix timestamp with validation and utility methods.
/// 
/// Supports comparison, normalization, and conversion to <see cref="DateTimeOffset"/>.
/// Valid range: 1970-01-01 to 2100-01-01 (Unix seconds 0–4102444800).
/// </summary>
internal readonly struct Timestamp : IComparable<Timestamp>
{
    /// <summary>
    /// Unix timestamp in seconds since 1970-01-01 UTC.
    /// </summary>
    public long UnixSeconds { get; }
    
    /// <summary>
    /// Corresponding <see cref="DateTimeOffset"/> in UTC.
    /// </summary>
    private DateTimeOffset DateTime => DateTimeOffset.FromUnixTimeSeconds(UnixSeconds);

    private Timestamp(long unixSeconds) => UnixSeconds = unixSeconds;
    private Timestamp(double unixSeconds) => UnixSeconds = (long)unixSeconds; 
    /// <summary>
    /// Creates <see cref="Timestamp"/> from Unix seconds.
    /// Returns failure if out of range (0–4102444800).
    /// </summary>
    /// <param name="unixSeconds">Unix seconds</param>
    /// <returns>Result with validated <see cref="Timestamp"/> or error</returns>
    public static Result<Timestamp> Create(long unixSeconds)
    {
        // Range: 1970 to 2100
        return unixSeconds is < 0 or > 4102444800
            ? Result<Timestamp>.Failure("Timestamp out of range")
            : Result<Timestamp>.Success(new Timestamp(unixSeconds));
    }

    /// <summary>
    /// Current UTC timestamp.
    /// </summary>
    public static Timestamp Now => new(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    /// <summary>
    /// Creates a <see cref="Timestamp"/> from a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="dateTime">DateTimeOffset value</param>
    /// <returns>Result with validated <see cref="Timestamp"/> or error</returns>
    public static Result<Timestamp> FromDateTime(DateTimeOffset dateTime)
        => Create(dateTime.ToUnixTimeSeconds());

    /// <summary>
    /// Returns a normalized timestamp relative to a base timestamp.
    /// </summary>
    /// <param name="baseTime">Base timestamp</param>
    /// <returns>Normalized timestamp (difference in seconds)</returns>
    public Timestamp Normalize(Timestamp baseTime, double scale = 1.0)
        => new((UnixSeconds - baseTime.UnixSeconds) * scale); 
    
    /// <summary>
    /// Compares this timestamp with another.
    /// </summary>
    /// <param name="other">Other timestamp</param>
    public int CompareTo(Timestamp other) => UnixSeconds.CompareTo(other.UnixSeconds);
    
    public override string ToString() => DateTime.ToString("yyyy-MM-dd HH:mm:ss");

    public static implicit operator long(Timestamp timestamp) => timestamp.UnixSeconds;
    
    public static bool operator <(Timestamp left, Timestamp right) => left.CompareTo(right) < 0;
    public static bool operator >(Timestamp left, Timestamp right) => left.CompareTo(right) > 0;
    public static bool operator <=(Timestamp left, Timestamp right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Timestamp left, Timestamp right) => left.CompareTo(right) >= 0;
}