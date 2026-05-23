using System.Numerics;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot representing particle for rendering purposes.
/// </summary>
/// <remarks>
/// Captures particle position, alpha transparency, size, and color.
/// Used to render particle systems consistently without mutating the live simulation state.
/// </remarks>
internal readonly record struct ParticleSnapshot(
    Vec2 Position,
    float Alpha,
    float Size,
    Vector4 Color
);