using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Helpers;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles <see cref="FileNodeCommand"/> by managing file nodes in scene.
/// </summary>
/// <remarks>
/// Supports spawning, pulsing glow, and removal of file nodes. Glow animations are
/// applied via provided <see cref="IAnimationSystem"/>. Node colors are
/// determined using <see cref="ColorPalette.ForFilePath"/>. Random offsets are
/// applied on spawn or pulse for visual spread.
/// </remarks>
internal sealed class FileNodeHandler(ISceneGraph scene, IAnimationSystem anim) : IRenderCommandHandler
{
    /// <summary>
    /// Duration in seconds for node glow pulse animations.
    /// </summary>
    private float NodeGlowDuration { get; set; } = 0.8f;

    /// <summary>
    /// Gets <see cref="RenderCommand"/> type this handler can process.
    /// </summary>
    public Type CommandType => typeof(FileNodeCommand);

    /// <summary>
    /// Processes given <see cref="FileNodeCommand"/> and updates scene accordingly.
    /// </summary>
    /// <param name="command">The render command to handle.</param>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (FileNodeCommand)command;
        switch (cmd.Action)
        {
            case FileNodeAction.Spawn:
            case FileNodeAction.Pulse:
            {
                var node = scene.GetOrAddNode(
                    cmd.FilePath,
                    NodeKind.File,
                    RenderingHelpers.RandomNear(),
                    ColorPalette.ForFilePath(cmd.FilePath));

                PulseGlow(node);
                break;
            }

            case FileNodeAction.Remove:
                scene.RemoveNode(cmd.FilePath);
                break;
        }
    }

    /// <summary>
    /// Triggers glow pulse on specified node with optional target color.
    /// </summary>
    /// <param name="node">Scene node to pulse.</param>
    /// <param name="targetGlow">Target glow intensity.</param>
    /// <param name="color">Optional override color.</param>
    private void PulseGlow(SceneNode node, float targetGlow = 1f, uint? color = null)
    {
        if (color.HasValue)
            node.Color = color.Value;

        node.Glow = targetGlow;

        anim.TweenFloat(targetGlow, 0f, NodeGlowDuration, Easing.EaseOutQuad,
            g => node.Glow = g);
    }
}