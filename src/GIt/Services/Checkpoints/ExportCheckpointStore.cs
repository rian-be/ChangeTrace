using System.Text.Json;
using System.Text.Json.Serialization;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Services.Checkpoints.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.GIt.Services.Checkpoints;

/// <summary>
/// Persists export checkpoints for resuming interrupted exports.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class ExportCheckpointStore(
    ISerializer<Timeline> serializer,
    IFileManager fileManager)
    : IExportCheckpointStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private const string MetadataFileName = "checkpoint.json";
    private const string SnapshotFileName = "checkpoint.timeline";
    private const string PatchLogFileName = "checkpoint.prlzog";

    /// <summary>
    /// Attempts to load a checkpoint for the given export key.
    /// </summary>
    public async Task<ExportCheckpointState?> TryLoad(
        string checkpointKey,
        string expectedFingerprint,
        CancellationToken cancellationToken = default)
    {
        var metadataPath = GetCheckpointFilePath(checkpointKey, MetadataFileName);
        if (!fileManager.Exists(metadataPath))
            return null;

        try
        {
            var metadataBytes = await fileManager.LoadAsync(metadataPath, cancellationToken);
            var metadata = JsonSerializer.Deserialize<ExportCheckpointDiskState>(metadataBytes, JsonOptions);
            if (metadata is null ||
                !string.Equals(metadata.Fingerprint, expectedFingerprint, StringComparison.Ordinal))
            {
                return null;
            }

            var snapshotPath = GetCheckpointFilePath(checkpointKey, SnapshotFileName);
            if (!fileManager.Exists(snapshotPath))
                return null;

            var snapshotBytes = await fileManager.LoadAsync(snapshotPath, cancellationToken);
            var timeline = await serializer.DeserializeAsync(snapshotBytes, cancellationToken);

            var patchLogPath = GetCheckpointFilePath(checkpointKey, PatchLogFileName);
            if (fileManager.Exists(patchLogPath))
            {
                var patchBytes = await fileManager.LoadAsync(patchLogPath, cancellationToken);
                var patches = JsonSerializer.Deserialize<List<ExportCheckpointPatch>>(patchBytes, JsonOptions) ?? [];

                foreach (var patch in patches)
                {
                    timeline.TryUpdateAt(
                        patch.TargetIndex,
                        _ => patch.UpdatedEvent.ToTraceEvent());
                }
            }

            return new ExportCheckpointState(
                metadata.Fingerprint,
                metadata.Stage,
                metadata.NextPullRequestPage,
                metadata.NextPullRequestIndex,
                timeline);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves the base snapshot for a checkpoint atomically.
    /// </summary>
    public async Task Save(
        string checkpointKey,
        ExportCheckpointState state,
        CancellationToken cancellationToken = default)
    {
        var metadata = new ExportCheckpointDiskState(
            state.Fingerprint,
            state.Stage,
            state.NextPullRequestPage,
            state.NextPullRequestIndex);

        var metadataBytes = JsonSerializer.SerializeToUtf8Bytes(metadata, JsonOptions);
        var snapshotBytes = await serializer.SerializeAsync(state.Timeline, CancellationToken.None);

        await using var transaction = new AtomicFileTransaction(fileManager);
        await transaction.WriteBytesAsync(GetCheckpointFilePath(checkpointKey, MetadataFileName), metadataBytes, cancellationToken);
        await transaction.WriteBytesAsync(GetCheckpointFilePath(checkpointKey, SnapshotFileName), snapshotBytes, cancellationToken);
        transaction.Delete(GetCheckpointFilePath(checkpointKey, PatchLogFileName));
        await transaction.CommitAsync();
    }

    /// <summary>
    /// Appends a pull request patch and updates the resume cursor.
    /// </summary>
    public async Task AppendPullRequestPatch(
        string checkpointKey,
        ExportCheckpointState state,
        int targetIndex,
        TraceEvent updatedEvent,
        CancellationToken cancellationToken = default)
    {
        var metadata = new ExportCheckpointDiskState(
            state.Fingerprint,
            ExportCheckpointStage.EnrichingPullRequests,
            state.NextPullRequestPage,
            state.NextPullRequestIndex);

        var patches = await LoadPatchesAsync(checkpointKey, cancellationToken);
        patches.Add(new ExportCheckpointPatch(targetIndex, ExportCheckpointTraceEventSnapshot.From(updatedEvent)));

        var metadataBytes = JsonSerializer.SerializeToUtf8Bytes(metadata, JsonOptions);
        var patchBytes = JsonSerializer.SerializeToUtf8Bytes(patches, JsonOptions);

        await using var transaction = new AtomicFileTransaction(fileManager);
        await transaction.WriteBytesAsync(GetCheckpointFilePath(checkpointKey, MetadataFileName), metadataBytes, cancellationToken);
        await transaction.WriteBytesAsync(GetCheckpointFilePath(checkpointKey, PatchLogFileName), patchBytes, cancellationToken);
        await transaction.CommitAsync();
    }

    /// <summary>
    /// Clears checkpoint artifacts if they exist.
    /// </summary>
    public async Task Clear(string checkpointKey, CancellationToken cancellationToken = default)
    {
        await using var transaction = new AtomicFileTransaction(fileManager);
        transaction.Delete(GetCheckpointFilePath(checkpointKey, MetadataFileName));
        transaction.Delete(GetCheckpointFilePath(checkpointKey, SnapshotFileName));
        transaction.Delete(GetCheckpointFilePath(checkpointKey, PatchLogFileName));
        await transaction.CommitAsync();

        CleanupDirectoryIfEmpty(checkpointKey);
    }

    private async Task<List<ExportCheckpointPatch>> LoadPatchesAsync(
        string checkpointKey,
        CancellationToken cancellationToken)
    {
        var patchLogPath = GetCheckpointFilePath(checkpointKey, PatchLogFileName);
        if (!fileManager.Exists(patchLogPath))
            return [];

        var bytes = await fileManager.LoadAsync(patchLogPath, cancellationToken);
        return JsonSerializer.Deserialize<List<ExportCheckpointPatch>>(bytes, JsonOptions) ?? [];
    }

    private static string GetCheckpointFilePath(string checkpointKey, string fileName)
        => Path.Combine(GetCheckpointDirectory(checkpointKey), fileName);

    private static string GetCheckpointDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        var sidecarFolder = Path.GetFileName(filePath) + ".parts";
        return Path.Combine(directory, sidecarFolder);
    }

    private void CleanupDirectoryIfEmpty(string checkpointKey)
    {
        var directory = GetCheckpointDirectory(checkpointKey);
        if (!Directory.Exists(directory))
            return;

        if (Directory.EnumerateFileSystemEntries(directory).Any())
            return;

        Directory.Delete(directory);
    }
}
