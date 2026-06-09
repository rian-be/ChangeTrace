namespace ChangeTrace.GIt.Services.Checkpoints.Models;

/// <summary>
/// Serialized patch entry for checkpointed pull request enrichment.
/// </summary>
internal sealed record ExportCheckpointPatch(
    int TargetIndex,
    ExportCheckpointTraceEventSnapshot UpdatedEvent);
