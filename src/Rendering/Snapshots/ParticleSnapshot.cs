namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot representing particle for rendering purposes.
/// </summary>
/// <remarks>
/// Captures particle position, alpha transparency, size, and color.
/// Used to render particle systems consistently without mutating live simulation state.
/// </remarks>
internal sealed record ParticleSnapshot(
    Vec2 Position,
    float Alpha,
    float Size,
    uint Color
);