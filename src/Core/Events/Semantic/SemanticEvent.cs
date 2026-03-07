namespace ChangeTrace.Core.Events.Semantic;

/// <summary>
/// Represents single semantic event with virtual timestamp.
/// </summary>
/// <remarks>
/// This is a lightweight, immutable struct intended for event streams.
/// </remarks>
internal readonly struct SemanticEvent(double timestamp)
{
    /// <summary>
    /// Gets virtual time of event.
    /// </summary>
    public readonly double Timestamp = timestamp;
}