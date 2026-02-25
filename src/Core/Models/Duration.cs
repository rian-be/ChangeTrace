namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents duration of time in seconds.
/// </summary>
/// <remarks>
/// Immutable value type. Supports scaling and conversion to total seconds.
/// </remarks>
internal readonly struct Duration
{
    /// <summary>Internal storage of seconds.</summary>
    private long Seconds { get; }

    /// <summary>
    /// Initializes new <see cref="Duration"/> instance w given number of seconds.
    /// </summary>
    /// <param name="seconds">Duration in seconds.</param>
    public Duration(long seconds) => Seconds = seconds;

    /// <summary>
    /// Returns new <see cref="Duration"/> scaled by given factor.
    /// </summary>
    /// <param name="factor">Multiplicative factor for scaling.</param>
    /// <returns>Scaled duration.</returns>
    public Duration Scale(double factor) =>
        new((long)(Seconds * factor));

    /// <summary>
    /// Gets the total duration in seconds.
    /// </summary>
    public double TotalSeconds => Seconds;

    /// <summary>
    /// Returns a string representation of the duration.
    /// </summary>
    public override string ToString() => $"{Seconds}s";
}