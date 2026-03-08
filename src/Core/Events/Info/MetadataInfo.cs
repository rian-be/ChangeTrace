using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Events.Info;

/// <summary>
/// Represents optional metadata associated with trace event.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Holds arbitrary string metadata via <see cref="Metadata"/>.</item>
/// <item>Optionally references a file path via <see cref="FilePath"/>.</item>
/// <item>Tracks last modification timestamps per actor via <see cref="LastModified"/>.</item>
/// <item>Provides helper <see cref="WithMetadata"/> to create a copy with updated metadata.</item>
/// </list>
/// </remarks>
internal readonly record struct MetadataInfo(
    string? Metadata = null,
    FilePath? FilePath = null,
    Dictionary<ActorName, Timestamp>? LastModified = null)
{
    /// <summary>
    /// Returns copy of this <see cref="MetadataInfo"/> with updated metadata string.
    /// </summary>
    /// <param name="newMetadata">The new metadata value.</param>
    /// <returns>A new <see cref="MetadataInfo"/> instance with <see cref="Metadata"/> updated.</returns>
    public MetadataInfo WithMetadata(string newMetadata)
        => this with { Metadata = newMetadata };
}