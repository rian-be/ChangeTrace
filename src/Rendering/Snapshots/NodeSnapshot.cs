using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of scene node for rendering purposes.
/// </summary>
/// <remarks>
/// Captures node ID, position, visual radius, color, glow intensity, and node type.
/// Used to render nodes consistently in visualization system without mutating live scene.
/// </remarks>
internal sealed record NodeSnapshot(
    string Id,
    Vec2 Position,
    float Radius,
    uint Color,
    float Glow,
    NodeKind Kind
);