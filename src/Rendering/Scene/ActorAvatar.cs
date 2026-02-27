using ChangeTrace.Core.Models;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Represents visual avatar of an actor in scene.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks current and target position for interpolation.</item>
/// <item>Maintains visual properties like <see cref="Color"/> and <see cref="Alpha"/>.</item>
/// <item>Tracks <see cref="LastSeen"/> virtual time for activity-based effects.</item>
/// <item><see cref="ActivityLevel"/> indicates recent activity (0 = idle, 1 = just moved) for glow/trail rendering.</item>
/// </list>
/// </remarks>
internal sealed class ActorAvatar
{
    /// <summary>
    /// Identifier of actor.
    /// </summary>
    internal ActorName Actor { get; }

    /// <summary>
    /// Current position of avatar in scene coordinates.
    /// </summary>
    internal Vec2 Position { get; set; }

    /// <summary>
    /// Target position used for smooth interpolation.
    /// </summary>
    internal Vec2 Target { get; set; }

    /// <summary>
    /// Visual color of avatar, deterministic from palette.
    /// </summary>
    internal uint Color { get; }

    /// <summary>
    /// Transparency of avatar (1 = opaque, 0 = invisible).
    /// </summary>
    internal float Alpha { get; set; } = 1f;

    /// <summary>
    /// Virtual time of last event associated with this actor.
    /// </summary>
    internal double LastSeen { get; set; }

    /// <summary>
    /// Activity level (0 = idle, 1 = just moved) used for glow/trail intensity.
    /// </summary>
    internal float ActivityLevel { get; set; }

    /// <summary>
    /// Initializes new <see cref="ActorAvatar"/>.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <param name="spawnPosition">Initial position in scene.</param>
    /// <param name="color">Deterministic color from palette.</param>
    internal ActorAvatar(ActorName actor, Vec2 spawnPosition, uint color)
    {
        Actor = actor;
        Position = Target = spawnPosition;
        Color = color;
    }
}