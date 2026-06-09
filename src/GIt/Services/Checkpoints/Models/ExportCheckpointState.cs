using ChangeTrace.Core.Timelines;

namespace ChangeTrace.GIt.Services.Checkpoints.Models;

/// <summary>
/// Represents a loaded export checkpoint.
/// </summary>
internal sealed record ExportCheckpointState(
    string Fingerprint,
    ExportCheckpointStage Stage,
    int NextPullRequestPage,
    int NextPullRequestIndex,
    Timeline Timeline);
