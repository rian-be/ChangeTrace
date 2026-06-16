using System.Numerics;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of the scene node for rendering purposes.
/// </summary>
/// <remarks>
/// Captures node ID, position, visual radius, color, glow intensity, and node type.
/// Used to render nodes consistently in visualization systems without mutating live scene.
/// </remarks>
internal readonly record struct NodeSnapshot(
    string Id,
    Vec2 Position,
    float Radius,
    Vector4 Color,
    float Glow,
    NodeKind Kind,
    string Label,
    bool IsParent,
    string? ParentId = null
);
