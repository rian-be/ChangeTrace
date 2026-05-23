using JetBrains.Annotations;

namespace ChangeTrace.Rendering.Hud;

/// <summary>
/// HUD snapshot containing runtime visualization statistics.
/// </summary>
[UsedImplicitly]
internal sealed record StatisticsHud(
    int ActiveActors,
    int TotalNodes,
    int EventsFired,
    int TotalEvents,
    int LoopCount,
    IReadOnlyList<ExtensionStat> Extensions);

/// <summary>
/// Aggregated file extension statistics entry.
/// </summary>
internal record ExtensionStat(
    string Extension,
    int Count);