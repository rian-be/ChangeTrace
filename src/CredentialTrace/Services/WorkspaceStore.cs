using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// File backed store for <see cref="WorkspaceProfile"/> objects.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Persists workspaces in memory and delegates persistence to underlying <see cref="IProfileStore{WorkspaceProfile}"/>.</item>
/// <item>Supports filtering by organization ID or organization name.</item>
/// <item>Provides CRUD operations: save, delete, get by ID, get by name, list all.</item>
/// <item>Registered as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton, typeof(IWorkspaceStore))]
internal sealed class WorkspaceStore(
    IProfileStore<OrganizationProfile> orgStore,
    IProfileStore<WorkspaceProfile> inner) : IWorkspaceStore
{
    public async Task<IReadOnlyList<WorkspaceProfile>> GetByOrganizationAsync(
        Ulid organizationId,
        CancellationToken ct = default)
    {
        var all = await inner.GetAllAsync(ct);

        return all
            .Where(w => w.OrganizationId == organizationId)
            .OrderBy(w => w.Name)
            .ToList();
    }

    public Task SaveAsync(WorkspaceProfile profile, CancellationToken ct = default)
        => inner.SaveAsync(profile, ct);

    public Task DeleteAsync(Ulid id, CancellationToken ct = default)
        => inner.DeleteAsync(id, ct);

    public Task<WorkspaceProfile?> GetAsync(Ulid id, CancellationToken ct = default)
        => inner.GetAsync(id, ct);

    public Task<WorkspaceProfile?> GetByNameAsync(string name, CancellationToken ct = default)
        => inner.GetByNameAsync(name, ct);

    public Task<IEnumerable<WorkspaceProfile>> GetAllAsync(CancellationToken ct = default)
        => inner.GetAllAsync(ct);

    public async Task<IReadOnlyList<WorkspaceProfile>> GetByNameOrganization(
        string organizationName,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(organizationName))
            return [];
        
        var org = await orgStore.GetByNameAsync(organizationName, ct);
        if (org == null)
            return [];
        
        var allWorkspaces = await inner.GetAllAsync(ct);
        return allWorkspaces
            .Where(ws => ws.OrganizationId == org.Id)
            .OrderBy(ws => ws.Name)
            .ToList();
    }
}