namespace ChangeTrace.CredentialTrace.Profiles;

/// <summary>
/// Describes a workspace-managed timeline export.
/// </summary>
internal sealed class WorkspaceTimelineMetadata
{
    /// <summary>
    /// Workspace that owns the timeline.
    /// </summary>
    public required Ulid WorkspaceId { get; init; }

    /// <summary>
    /// Workspace display name at export time.
    /// </summary>
    public required string WorkspaceName { get; init; }

    /// <summary>
    /// Original repository source used for export.
    /// </summary>
    public required string RepositorySource { get; init; }

    /// <summary>
    /// Repository owner when resolved from a remote source.
    /// </summary>
    public string? RepositoryOwner { get; init; }

    /// <summary>
    /// Repository name resolved from the source.
    /// </summary>
    public string? RepositoryName { get; init; }

    /// <summary>
    /// Export creation time in UTC.
    /// </summary>
    public required DateTimeOffset ExportedAtUtc { get; init; }

    /// <summary>
    /// Full path to the timeline file.
    /// </summary>
    public required string TimelinePath { get; init; }
}
