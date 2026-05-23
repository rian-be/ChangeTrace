using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Helpers;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles actor avatar movement commands.
/// </summary>
internal sealed class MoveActorHandler(
    ISceneGraph scene,
    IAnimationSystem anim,
    IRenderStateAssembler assembler)
    : IRenderCommandHandler
{
    /// <summary>
    /// Duration of avatar movement animation.
    /// </summary>
    private float AvatarMoveDuration { get; set; } =  1.2f;

    /// <summary>
    /// Supported render command type.
    /// </summary>
    public Type CommandType =>
        typeof(MoveActorCommand);

    /// <summary>
    /// Applies actor movement command to the scene graph.
    /// </summary>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var (_, actorName, targetNodeId, isSpawn, commitSha) = (MoveActorCommand)command;

        var targetNode = scene.FindNode(targetNodeId);
        if (targetNode == null) 
            return;

        var color = ColorPalette.ForActor(actorName);

        var spawnPos = isSpawn
            ? RenderingHelpers.RandomEdge()
            : scene.FindAvatar(actorName)?.Position
              ?? RenderingHelpers.RandomEdge();

        var avatar = scene.GetOrAddAvatar(actorName, spawnPos, color);

        // Smart Cooldown: If this avatar just performed an action in the SAME virtual time slice,
        // don't perform a full reset. This prevents "immortality" in dense event streams.
        if (Math.Abs(avatar.LastSeen - virtualTime) < 0.001 && !isSpawn && avatar.TargetNodeId == targetNodeId)
            return;

        // If already at target, only bump activity slightly to allow decay to eventually win
        if (avatar.TargetNodeId == targetNodeId && !isSpawn)
        {
            avatar.LastSeen = virtualTime;
            avatar.ActivityLevel = Math.Min(1.0f, avatar.ActivityLevel + 0.2f);
            return;
        }

        avatar.LastSeen = virtualTime;
        avatar.ActivityLevel = 1f;
        avatar.TargetNodeId = targetNodeId;

        // Drift target with offset
        var offset = RenderingHelpers.RandomNear() * 20f;
        var to = targetNode.Position + offset;
        avatar.Target = to;

        var from = avatar.Position;

        anim.TweenVec2(from, to, AvatarMoveDuration, Easing.EaseOutCubic,
            pos => avatar.Position = pos, null, actorName.Value);

        assembler.RecordActorEvent(actorName.Value, commitSha);
    }
}