namespace ChangeTrace.Rendering.Commands;

/// <summary>
/// Command to trigger a particle burst at a specific node in the graph, typically for merge commits or PR merges.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="Timestamps"/> – Simulation time when the burst occurs.</item>
/// <item><see cref="AtNode"/> – Node (file or branch) where the particles originate.</item>
/// <item><see cref="ParticleCount"/> – Number of particles to spawn.</item>
/// <item><see cref="ColorRgb"/> – Packed RGB color of the particles.</item>
/// </list>
/// </remarks>
internal sealed record ParticleBurstCommand(
    double Timestamps,
    string AtNode,
    int ParticleCount,
    uint ColorRgb
) : RenderCommand(Timestamps);