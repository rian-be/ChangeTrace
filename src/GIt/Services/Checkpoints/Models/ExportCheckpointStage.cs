namespace ChangeTrace.GIt.Services.Checkpoints.Models;

/// <summary>
/// Export checkpoint stage.
/// </summary>
internal enum ExportCheckpointStage
{
    Built,
    EnrichingPullRequests,
    SavingTimeline,
    Enriched
}
