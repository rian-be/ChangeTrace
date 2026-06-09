using System.Text.Json;
using System.Text.Json.Serialization;
using ChangeTrace.Core.Timelines;
using ChangeTrace.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services.Sidecars;

/// <summary>
/// Base handler for timeline sidecar persistence and hydration.
/// </summary>
internal abstract class TimelineSidecarHandler<TSidecar>(
    ILogger logger,
    IFileManager fileManager)
    where TSidecar : class
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Gets the shared file manager.
    /// </summary>
    protected IFileManager FileManager { get; } = fileManager;

    /// <summary>
    /// Gets the shared logger.
    /// </summary>
    protected ILogger Logger { get; } = logger;

    /// <summary>
    /// Gets the sidecar file name inside the parts folder.
    /// </summary>
    protected abstract string SidecarFileName { get; }

    /// <summary>
    /// Gets the sidecar display name used in logs.
    /// </summary>
    protected abstract string DisplayName { get; }

    /// <summary>
    /// Builds the sidecar payload from a timeline.
    /// </summary>
    protected abstract TSidecar BuildSidecar(Timeline timeline);

    /// <summary>
    /// Gets whether the sidecar contains attachments.
    /// </summary>
    protected abstract bool HasAttachments(TSidecar sidecar);

    /// <summary>
    /// Gets the attachment count for logging.
    /// </summary>
    protected abstract int GetAttachmentCount(TSidecar sidecar);

    /// <summary>
    /// Serializes the sidecar payload.
    /// </summary>
    protected abstract byte[] Serialize(TSidecar sidecar);

    /// <summary>
    /// Deserializes the sidecar payload.
    /// </summary>
    protected abstract TSidecar Deserialize(byte[] bytes);

    /// <summary>
    /// Applies the sidecar payload to a timeline.
    /// </summary>
    protected abstract int ApplyToTimeline(TSidecar sidecar, Timeline timeline);

    /// <summary>
    /// Creates the debug snapshot payload.
    /// </summary>
    protected abstract object CreateDebugSnapshot(TSidecar sidecar);

    /// <summary>
    /// Persists the sidecar and optional debug snapshot.
    /// </summary>
    public async Task PersistAsync(
        AtomicFileTransaction transaction,
        Timeline timeline,
        string timelinePath,
        bool writeDebugSnapshot,
        CancellationToken cancellationToken)
    {
        var sidecar = BuildSidecar(timeline);
        var sidecarPath = GetSidecarPath(timelinePath);

        if (!HasAttachments(sidecar))
        {
            DeleteArtifacts(transaction, timelinePath);
            return;
        }

        await transaction.WriteBytesAsync(sidecarPath, Serialize(sidecar), cancellationToken);

        if (writeDebugSnapshot)
            await WriteDebugSnapshotAsync(transaction, sidecar, sidecarPath, cancellationToken);

        Logger.LogInformation(
            "{DisplayName} prepared at {Path} ({Count} attachments)",
            DisplayName,
            sidecarPath,
            GetAttachmentCount(sidecar));
    }

    /// <summary>
    /// Applies the sidecar from disk to the timeline if present.
    /// </summary>
    public async Task<int> ApplyAsync(
        string timelinePath,
        Timeline timeline,
        CancellationToken cancellationToken)
    {
        if (IsSidecarPath(timelinePath))
            return 0;

        var sidecarPath = GetSidecarPath(timelinePath);
        if (!FileManager.Exists(sidecarPath))
            return 0;

        try
        {
            var bytes = await FileManager.LoadAsync(sidecarPath, cancellationToken);
            var sidecar = Deserialize(bytes);
            var applied = ApplyToTimeline(sidecar, timeline);

            Logger.LogInformation(
                "Applied {DisplayName} from {Path} ({Count} attachments)",
                DisplayName,
                sidecarPath,
                applied);

            return applied;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(
                ex,
                "Failed to load {DisplayName} from {Path}; continuing without it",
                DisplayName,
                sidecarPath);
            return 0;
        }
    }

    /// <summary>
    /// Stages removal of sidecar artifacts.
    /// </summary>
    public void DeleteArtifacts(AtomicFileTransaction transaction, string timelinePath)
    {
        var sidecarPath = GetSidecarPath(timelinePath);
        transaction.Delete(sidecarPath);
        transaction.Delete(GetDebugPath(sidecarPath));
    }

    /// <summary>
    /// Removes empty sidecar directories after commit.
    /// </summary>
    public void Cleanup(string timelinePath)
    {
        var sidecarPath = GetSidecarPath(timelinePath);
        var directory = Path.GetDirectoryName(sidecarPath);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return;

        if (Directory.EnumerateFileSystemEntries(directory).Any())
            return;

        Directory.Delete(directory);
    }

    private async Task WriteDebugSnapshotAsync(
        AtomicFileTransaction transaction,
        TSidecar sidecar,
        string sidecarPath,
        CancellationToken cancellationToken)
    {
        var snapshot = CreateDebugSnapshot(sidecar);
        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        await transaction.WriteBytesAsync(GetDebugPath(sidecarPath), bytes, cancellationToken);
    }

    private static bool IsSidecarPath(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        return fileName.StartsWith("pullrequest.", StringComparison.OrdinalIgnoreCase) ||
               fileName.StartsWith("merge.", StringComparison.OrdinalIgnoreCase);
    }

    protected string GetSidecarPath(string filePath)
        => Path.Combine(GetSidecarDirectory(filePath), SidecarFileName);

    protected static string GetDebugPath(string sidecarPath)
        => sidecarPath + ".debug.json";

    protected static string GetSidecarDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var sidecarFolder = Path.GetFileName(filePath) + ".parts";
        return Path.Combine(directory, sidecarFolder);
    }
}
