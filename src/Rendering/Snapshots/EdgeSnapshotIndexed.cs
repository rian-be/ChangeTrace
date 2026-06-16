using System.Numerics;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of a scene edge for rendering purposes.
/// </summary>
/// <remarks>
/// Stores node indices instead of node id strings to reduce per-edge snapshot size
/// while keeping enough data to resolve endpoints through the scene snapshot.
/// </remarks>
internal readonly record struct EdgeSnapshotIndexed(
    int FromIndex,
    int ToIndex,
    EdgeKind Kind,
    float Alpha,
    Vector4 Color,
    float WidthStart = 1.0f,
    float WidthEnd = 1.0f
);
