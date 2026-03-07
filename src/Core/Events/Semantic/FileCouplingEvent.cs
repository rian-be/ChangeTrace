namespace ChangeTrace.Core.Events.Semantic;

/// <summary>
/// Represents coupling relation between two files that were modified together in the same commit.
/// Useful for building file dependency graphs and performing architectural analysis.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the event's virtual time via <see cref="Timestamp"/>.</item>
/// <item>Records the two files involved in the coupling via <see cref="FileA"/> and <see cref="FileB"/>.</item>
/// <item>Can be implicitly converted to <see cref="SemanticEvent"/> to integrate with generic semantic pipelines.</item>
/// </list>
/// </remarks>
internal readonly struct FileCouplingEvent(
    double timestamp,
    string fileA,
    string fileB)
{
    /// <summary>
    /// Gets the event timestamp (Unix seconds).
    /// </summary>
    public readonly double Timestamp = timestamp;

    /// <summary>
    /// Gets the first file in coupling relation.
    /// </summary>
    public readonly string FileA = fileA;

    /// <summary>
    /// Gets the second file in the coupling relation.
    /// </summary>
    public readonly string FileB = fileB;

    /// <summary>
    /// Implicitly converts this event to a <see cref="SemanticEvent"/>.
    /// Enables writing to generic semantic pipelines.
    /// </summary>
    /// <param name="e">The file coupling event.</param>
    public static implicit operator SemanticEvent(FileCouplingEvent e)
        => new(e.Timestamp);
}