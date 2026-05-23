namespace ChangeTrace.Core.Events.Semantic;

/// <summary>
/// Represents an activity intensity event for file, eg hotspot based on recent changes.
/// Useful for rendering heatmaps and highlighting active files in the repository.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the virtual time of the event via <see cref="Timestamp"/>.</item>
/// <item>Records the file path affected via <see cref="File"/>.</item>
/// <item>Records the activity intensity (e.g., number of changes) via <see cref="Activity"/>.</item>
/// <item>Can be implicitly converted to <see cref="SemanticEvent"/> to integrate with generic semantic pipelines.</item>
/// </list>
/// </remarks>
internal readonly struct FileHotspotEvent(
    double timestamp,
    string file,
    int activity)
{
    /// <summary>Gets the event timestamp (Unix seconds).</summary>
    public readonly double Timestamp = timestamp;

    /// <summary>Gets the file path for this hotspot event.</summary>
    public readonly string File = file;

    /// <summary>Gets the activity intensity of this file.</summary>
    public readonly int Activity = activity;

    /// <summary>Implicitly converts this event to a <see cref="SemanticEvent"/>.</summary>
    /// <param name="e">The file hotspot event.</param>
    public static implicit operator SemanticEvent(FileHotspotEvent e)
        => new(e.Timestamp);
}