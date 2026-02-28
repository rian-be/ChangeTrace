using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles <see cref="ParticleBurstCommand"/> by triggering particle burst effect at node.
/// </summary>
/// <remarks>
/// If the target node exists in <see cref="ISceneGraph"/>, burst animation is
/// created using the <see cref="IAnimationSystem"/>. The number of particles and
/// color are specified by the command.
/// </remarks>
internal sealed class ParticleBurstHandler(ISceneGraph scene, IAnimationSystem anim) : IRenderCommandHandler
{
    /// <summary>
    /// Gets <see cref="RenderCommand"/> type this handler can process.
    /// </summary>
    public Type CommandType => typeof(ParticleBurstCommand);

    /// <summary>
    /// Processes  given <see cref="ParticleBurstCommand"/> and triggers particle effect.
    /// </summary>
    /// <param name="command">The command containing node, color, and particle count.</param>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (ParticleBurstCommand)command;

        var node = scene.FindNode(cmd.AtNode);
        if (node == null) 
            return;

        anim.Burst(node.Position, cmd.ParticleCount, cmd.ColorRgb);
    }
} 