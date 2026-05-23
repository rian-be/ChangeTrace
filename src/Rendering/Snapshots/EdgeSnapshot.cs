using System.Numerics;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of scene edge for rendering purposes.
/// </summary>
/// <remarks>
/// Captures source and target node IDs, edge type, current alpha transparency, 
/// and deterministic color. Used to render edges in the visualization system.
/// </remarks>
internal readonly record struct EdgeSnapshot(
    string FromId,
    string ToId,
    EdgeKind Kind,
    float Alpha,
    Vector4 Color,
    float WidthStart = 1.0f,
    float WidthEnd = 1.0f
);