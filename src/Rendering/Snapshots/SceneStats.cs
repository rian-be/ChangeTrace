namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Simple statistics about scene snapshot.
/// </summary>
internal sealed record SceneStats(
    int NodeCount,
    int AvatarCount,
    int EdgeCount,
    int ParticleCount,
    int ActiveAvatars,
    int GlowingNodes,
    int VisibleEdges
);