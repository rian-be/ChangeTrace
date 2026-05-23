using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.States.Particles;

/// <summary>
/// Builds immutable particle snapshots from the animation system.
/// </summary>
internal sealed class ParticleSnapshotAssembler
{
    /// <summary>
    /// Captures the current particle state for rendering.
    /// </summary>
    public List<ParticleSnapshot> Assemble(
        IAnimationSystem animationSystem)
    {
        var snapshots =
            new List<ParticleSnapshot>(
                animationSystem.ParticleCount);

        animationSystem.SnapshotParticles(
            snapshots);

        return snapshots;
    }
}