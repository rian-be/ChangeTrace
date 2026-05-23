using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles pull request badge highlight commands.
/// </summary>
internal sealed class PullRequestBadgeHandler(
    ISceneGraph scene,
    IAnimationSystem anim)
    : IRenderCommandHandler
{
    /// <summary>
    /// Duration of glow fade animation.
    /// </summary>
    private float NodeGlowDuration { get; set; } =
        0.8f;

    /// <summary>
    /// Supported render command type.
    /// </summary>
    public Type CommandType =>
        typeof(PullRequestBadgeCommand);

    /// <summary>
    /// Applies pull request highlight effect to the branch node.
    /// </summary>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (PullRequestBadgeCommand)command;
        var node = scene.FindNode(cmd.BranchName);
        if (node == null) return;
        
        node.Color = ColorPalette.UIntToVec4(0xCE93D8);
        node.Glow = 1f;

        anim.TweenFloat(1f, 0f, NodeGlowDuration, Easing.EaseOutQuad,
            g => node.Glow = g);
    }
}