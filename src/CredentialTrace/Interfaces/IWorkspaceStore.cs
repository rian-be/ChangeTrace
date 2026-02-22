using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;

namespace ChangeTrace.CredentialTrace.Interfaces;

/// <summary>
/// Store for <see cref="WorkspaceProfile"/> objects.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Supports CRUD operations: save, delete, get by ID, get by name, list all.</item>
/// <item>Provides method to list workspaces by organization name.</item>
/// <item>Designed for dependency injection and can be implemented with file, memory, or DB backing.</item>
/// </list>
/// </remarks>
internal interface IWorkspaceStore 
    : IProfileStore<WorkspaceProfile>
{
    /// <summary>
    /// Lists workspaces associated with organization name.
    /// Returns empty if none found or name is null/whitespace.
    /// </summary>
    /// <param name="organizationName">Organization name to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Read-only list of workspaces.</returns>
    Task<IReadOnlyList<WorkspaceProfile>> GetByNameOrganization(
        string organizationName,
        CancellationToken ct = default);
}