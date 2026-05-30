namespace ChangeTrace.CredentialTrace.Profiles;

/// <summary>
/// Represents a timeline file stored for a workspace.
/// </summary>
internal sealed class WorkspaceTimelineFile
{
    /// <summary>
    /// Full path to the timeline file.
    /// </summary>
    public required string Path { get; init; }

    /// <summary>
    /// Timeline file name.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// Last modified time in UTC.
    /// </summary>
    public DateTimeOffset LastModifiedUtc { get; init; }

    /// <summary>
    /// Metadata loaded from the sidecar file.
    /// </summary>
    public WorkspaceTimelineMetadata? Metadata { get; init; }
}
