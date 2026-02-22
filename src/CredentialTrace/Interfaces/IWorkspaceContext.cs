using ChangeTrace.CredentialTrace.Profiles;

namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Provides access to currently active workspace and allows changing it.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Exposes currently selected workspace, or null if none is active.</item>
/// <item>Supports changing active workspace and persisting the selection.</item>
/// </list>
/// </remarks>
internal interface IWorkspaceContext
{
    /// <summary>
    /// Currently active workspace. Null if none is selected.
    /// </summary>
    WorkspaceProfile? Current { get; }

    /// <summary>
    /// Sets active workspace and persists selection.
    /// </summary>
    /// <param name="workspace">Workspace to activate.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetCurrentAsync(WorkspaceProfile workspace, CancellationToken ct = default);
}