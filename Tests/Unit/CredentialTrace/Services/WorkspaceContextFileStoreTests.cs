using ChangeTrace.Core.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;
using ChangeTrace.Tests.TestDoubles;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Services;

/// <summary>Tests active workspace persistence through WorkspaceContextFileStore.</summary>
public sealed class WorkspaceContextFileStoreTests
{
    /// <summary>SetCurrentAsync updates Current and persists the workspace id.</summary>
    [Fact]
    public async Task SetCurrentAsync_UpdatesCurrentAndPersistsWorkspaceId()
    {
        var workspace = WorkspaceProfile.Create(Ulid.NewUlid(), "Backend");
        var fileManager = new InMemoryFileManager();
        var serializer = new UlidTestSerializer();
        var context = new WorkspaceContextFileStore(
            fileManager,
            new InMemoryProfileStore<WorkspaceProfile>(workspace),
            serializer);

        await context.SetCurrentAsync(workspace);

        Assert.Same(workspace, context.Current);
        Assert.Equal(".changetrace/current_workspace.json", fileManager.SavedPath);
        Assert.Equal(workspace.Id.ToString(), serializer.SerializedValue?.ToString());
    }

    /// <summary>Constructor loads an existing workspace id and resolves it from the profile store.</summary>
    [Fact]
    public void Constructor_LoadsCurrentWorkspaceWhenConfigExists()
    {
        var workspace = WorkspaceProfile.Create(Ulid.NewUlid(), "Backend");
        var serializer = new UlidTestSerializer { DeserializedValue = workspace.Id };
        var fileManager = new InMemoryFileManager
        {
            ExistingPath = ".changetrace/current_workspace.json",
            BytesToLoad = [1, 2, 3]
        };

        var context = new WorkspaceContextFileStore(
            fileManager,
            new InMemoryProfileStore<WorkspaceProfile>(workspace),
            serializer);

        Assert.Same(workspace, context.Current);
        Assert.Equal(".changetrace/current_workspace.json", fileManager.LoadedPath);
    }

    /// <summary>File manager test double for current-workspace config access.</summary>
    private sealed class InMemoryFileManager : IFileManager
    {
        /// <summary>Path treated as existing by Exists.</summary>
        public string? ExistingPath { get; init; }

        /// <summary>Bytes returned by LoadAsync.</summary>
        public byte[] BytesToLoad { get; init; } = [];

        /// <summary>Path passed to SaveAsync.</summary>
        public string? SavedPath { get; private set; }

        /// <summary>Bytes passed to SaveAsync.</summary>
        public byte[]? SavedBytes { get; private set; }

        /// <summary>Path passed to LoadAsync.</summary>
        public string? LoadedPath { get; private set; }

        /// <summary>Records saved bytes.</summary>
        public Task SaveAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            SavedPath = path;
            SavedBytes = data;
            return Task.CompletedTask;
        }

        /// <summary>Records load path and returns configured bytes.</summary>
        public Task<byte[]> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            LoadedPath = path;
            return Task.FromResult(BytesToLoad);
        }

        /// <summary>Returns true only for configured existing path.</summary>
        public bool Exists(string path) => path == ExistingPath;

        /// <summary>Returns fallback text for unsupported text reads.</summary>
        public Task<string> ReadAllTextAsync(string path, string fallback = "", CancellationToken ct = default)
            => Task.FromResult(fallback);

        /// <summary>Ensures a suffix extension on a path.</summary>
        public string EnsureExtension(string path, string extension)
            => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? path : path + extension;

        /// <summary>Returns no files for unsupported file discovery.</summary>
        public IEnumerable<string> FindFiles(string directory, string extension) => [];
    }

    /// <summary>Ulid serializer test double for active workspace ids.</summary>
    private sealed class UlidTestSerializer : ISerializer<Ulid>
    {
        /// <summary>Value passed to SerializeAsync.</summary>
        public Ulid? SerializedValue { get; private set; }

        /// <summary>Value returned by DeserializeAsync.</summary>
        public Ulid DeserializedValue { get; init; }

        /// <summary>Records serialized value and returns deterministic bytes.</summary>
        public Task<byte[]> SerializeAsync(Ulid obj, CancellationToken ct = default)
        {
            SerializedValue = obj;
            return Task.FromResult<byte[]>([4, 5, 6]);
        }

        /// <summary>Returns the configured deserialized value.</summary>
        public Task<Ulid> DeserializeAsync(byte[] data, CancellationToken ct = default)
            => Task.FromResult(DeserializedValue);
    }
}
