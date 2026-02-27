using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of scene edge for rendering purposes.
/// </summary>
/// <remarks>
/// Captures source and target node IDs, edge type, current alpha transparency, 
/// and deterministic color. Used to render edges in the visualization system.
/// </remarks>
internal sealed record EdgeSnapshot(
    string FromId,
    string ToId,
    EdgeKind Kind,
    float Alpha,
    uint Color
);