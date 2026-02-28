using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles <see cref="PullRequestBadgeCommand"/> by highlighting branch node
/// with pull request badge effect.
/// </summary>
/// <remarks>
/// If target branch node exists in <see cref="ISceneGraph"/>, its color is
/// updated and glow animation is triggered using <see cref="IAnimationSystem"/>.
/// The glow fades out over <c>NodeGlowDuration</c> seconds using easing.
/// </remarks>
internal sealed class PullRequestBadgeHandler(ISceneGraph scene, IAnimationSystem anim) : IRenderCommandHandler
{
    /// <summary>
    /// Duration in seconds for node glow animation.
    /// </summary>
    private float NodeGlowDuration { get; set; } = 0.8f;

    /// <summary>
    /// Gets <see cref="RenderCommand"/> type this handler can process.
    /// </summary>
    public Type CommandType => typeof(PullRequestBadgeCommand);

    /// <summary>
    /// Processes <see cref="PullRequestBadgeCommand"/> and applies glow effect to branch node.
    /// </summary>
    /// <param name="command">The pull request badge command.</param>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (PullRequestBadgeCommand)command;

        var node = scene.FindNode(cmd.BranchName);
        if (node == null) return;

        node.Color = 0xCE93D8;
        node.Glow = 1f;

        anim.TweenFloat(1f, 0f, NodeGlowDuration, Easing.EaseOutQuad,
            g => node.Glow = g);
    }
}