using System.Numerics;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;

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
    NodeFlyweight Flyweight,
    string Label,
    bool IsParent,
    string? ParentId = null
)
{
    public NodeKind Kind => Flyweight.Kind;
}