using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles particle burst effect commands.
/// </summary>
internal sealed class ParticleBurstHandler(
    ISceneGraph scene,
    IAnimationSystem anim)
    : IRenderCommandHandler
{
    /// <summary>
    /// Supported render command type.
    /// </summary>
    public Type CommandType =>
        typeof(ParticleBurstCommand);

    /// <summary>
    /// Applies particle burst command to the scene graph.
    /// </summary>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (ParticleBurstCommand)command;
        var colorVec = ColorPalette.UIntToVec4(cmd.ColorRgb);
        var node = scene.FindNode(cmd.AtNode);
        if (node == null) 
            return;

        anim.Burst(node.Position, cmd.ParticleCount, colorVec);
    }
} 