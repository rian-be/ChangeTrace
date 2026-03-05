using System.Numerics;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of an actor avatar at specific point in time.
/// </summary>
/// <remarks>
/// Captures actor identifier, position, color, transparency (alpha),
/// and activity level for rendering or HUD purposes.
/// </remarks>
internal sealed record AvatarSnapshot(
    string Actor,
    Vec2 Position,
    Vector4 Color,
    float Alpha,
    float ActivityLevel
);