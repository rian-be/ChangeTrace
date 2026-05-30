using ChangeTrace.CredentialTrace.Profiles;

namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Stores and lists timeline files for workspaces.
/// </summary>
internal interface IWorkspaceTimelineStorage
{
    /// <summary>
    /// Creates workspace specific timeline path for an export.
    /// </summary>
    Task<string> CreateTimelinePathAsync(
        WorkspaceProfile workspace,
        string repositorySource,
        DateTimeOffset exportedAt,
        string uniqueId,
        CancellationToken ct = default);

    /// <summary>
    /// Saves metadata next to timeline file.
    /// </summary>
    Task SaveMetadataAsync(
        string timelinePath,
        WorkspaceProfile workspace,
        string repositorySource,
        DateTimeOffset exportedAt,
        CancellationToken ct = default);

    /// <summary>
    /// Lists timeline files stored for the workspace.
    /// </summary>
    Task<IReadOnlyList<WorkspaceTimelineFile>> ListTimelinesAsync(
        WorkspaceProfile workspace,
        CancellationToken ct = default);
}
