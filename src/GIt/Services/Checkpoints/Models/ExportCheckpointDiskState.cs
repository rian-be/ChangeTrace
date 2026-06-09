namespace ChangeTrace.GIt.Services.Checkpoints.Models;

/// <summary>
/// Serialized checkpoint metadata.
/// </summary>
internal sealed record ExportCheckpointDiskState(
    string Fingerprint,
    ExportCheckpointStage Stage,
    int NextPullRequestPage,
    int NextPullRequestIndex);
