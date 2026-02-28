using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Helpers;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles <see cref="MoveActorCommand"/> by moving actor avatars in the scene graph.
/// </summary>
/// <remarks>
/// Actors are moved towards the target node with a smooth tween animation. If the
/// actor is spawning, a random start position is chosen near scene edges. Activity
/// levels and timestamps are updated, and the <see cref="IRenderStateAssembler"/>
/// records the actor event for telemetry or HUD purposes.
/// </remarks>
internal sealed class MoveActorHandler(
    ISceneGraph scene, 
    IAnimationSystem anim, 
    IRenderStateAssembler assembler
) : IRenderCommandHandler
{
    /// <summary>
    /// Duration in seconds for avatar movement animations.
    /// </summary>
    private float AvatarMoveDuration { get; set; } = 0.4f;

    /// <summary>
    /// Gets <see cref="RenderCommand"/> type this handler can process.
    /// </summary>
    public Type CommandType => typeof(MoveActorCommand);

    /// <summary>
    /// Processes given <see cref="MoveActorCommand"/> and updates actor avatar.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var (_, actorName, targetNodeId, isSpawn) = (MoveActorCommand)command;

        var targetNode = scene.FindNode(targetNodeId);
        if (targetNode == null) 
            return;

        var color = ColorPalette.ForActor(actorName);

        var spawnPos = isSpawn
            ? RenderingHelpers.RandomEdge()
            : scene.FindAvatar(actorName)?.Position
              ?? RenderingHelpers.RandomEdge();

        var avatar = scene.GetOrAddAvatar(actorName, spawnPos, color);

        avatar.LastSeen = virtualTime;
        avatar.ActivityLevel = 1f;
        avatar.Target = targetNode.Position;

        var from = avatar.Position;
        var to = targetNode.Position;

        anim.TweenVec2(from, to, AvatarMoveDuration, Easing.EaseOutCubic,
            pos => avatar.Position = pos);

        assembler.RecordActorEvent(actorName.Value);
    }
}