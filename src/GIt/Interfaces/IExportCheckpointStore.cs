using ChangeTrace.Core.Events;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Services;
using ChangeTrace.GIt.Services.Checkpoints.Models;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Persists and restores export checkpoints.
/// </summary>
internal interface IExportCheckpointStore
{
    Task<ExportCheckpointState?> TryLoad(
        string checkpointKey,
        string expectedFingerprint,
        CancellationToken cancellationToken = default);

    Task Save(
        string checkpointKey,
        ExportCheckpointState state,
        CancellationToken cancellationToken = default);

    Task AppendPullRequestPatch(
        string checkpointKey,
        ExportCheckpointState state,
        int targetIndex,
        TraceEvent updatedEvent,
        CancellationToken cancellationToken = default);

    Task Clear(
        string checkpointKey,
        CancellationToken cancellationToken = default);
}
