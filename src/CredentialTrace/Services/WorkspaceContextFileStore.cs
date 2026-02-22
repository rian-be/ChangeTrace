using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.CredentialTrace.Services;

/// <summary>
/// Workspace context implementation that persists the currently active workspace to a file.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Registered as singleton via <see cref="ISerializer{T}"/>.</item>
/// <item>Loads current workspace from disk at initialization.</item>
/// <item>Allows changing active workspace and persisting the selection.</item>
/// <item>Uses <see cref="AutoRegisterAttribute"/> for file operations and <see cref="IFileManager"/> for Ulid serialization.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkspaceContextFileStore : IWorkspaceContext
{
    private readonly IFileManager _fileManager;
    private readonly IProfileStore<WorkspaceProfile> _workspaceStore;
    private readonly ISerializer<Ulid> _idSerializer;
    
    private const string ConfigFile = ".changetrace/current_workspace.json";

    /// <inheritdoc />
    public WorkspaceProfile? Current { get; private set; }

    /// <summary>
    /// Initializes workspace context and loads current workspace from disk if available.
    /// </summary>
    public WorkspaceContextFileStore(
        IFileManager fileManager,
        IProfileStore<WorkspaceProfile> workspaceStore,
        ISerializer<Ulid> idSerializer)
    {
        _fileManager = fileManager;
        _workspaceStore = workspaceStore;
        _idSerializer = idSerializer;
        
        LoadCurrentWorkspace().Wait();
    }

    /// <summary>
    /// Loads current workspace from config file if it exists.
    /// </summary>
    private async Task LoadCurrentWorkspace()
    {
        if (!_fileManager.Exists(ConfigFile))
            return;

        var data = await _fileManager.LoadAsync(ConfigFile, CancellationToken.None);
        if (data.Length == 0) return;

        var id = await _idSerializer.DeserializeAsync(data);
        if (id == default) return;

        Current = await _workspaceStore.GetAsync(id, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task SetCurrentAsync(WorkspaceProfile workspace, CancellationToken ct = default)
    {
        Current = workspace;

        var data = await _idSerializer.SerializeAsync(workspace.Id, ct);
        await _fileManager.SaveAsync(ConfigFile, data, ct);
    }
}