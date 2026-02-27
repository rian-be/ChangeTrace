using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Represents node in scene graph.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks node identity (<see cref="Id"/>) and type (<see cref="Kind"/>).</item>
/// <item>Maintains physics-related properties: <see cref="Position"/>, <see cref="Velocity"/>, <see cref="Mass"/>.</item>
/// <item>Visual properties: <see cref="Radius"/>, <see cref="Color"/>, <see cref="Glow"/>.</item>
/// <item>Pinned nodes (<see cref="Pinned"/>) are excluded from physics simulations.</item>
/// </list>
/// </remarks>
internal sealed class SceneNode
{
    /// <summary>
    /// Unique identifier of node.
    /// </summary>
    internal string Id { get; }

    /// <summary>
    /// Type of node (root, branch, file).
    /// </summary>
    internal NodeKind Kind { get; }

    /// <summary>
    /// Current position in 2D space.
    /// </summary>
    internal Vec2 Position { get; set; }

    /// <summary>
    /// Current velocity; used for force directed layout calculations.
    /// </summary>
    internal Vec2 Velocity { get; set; }

    /// <summary>
    /// Mass of node for physics simulation.
    /// </summary>
    internal float Mass { get; }

    /// <summary>
    /// Radius of node; derived from <see cref="Kind"/>.
    /// </summary>
    internal float Radius { get; }

    /// <summary>
    /// Color of node (packed RGB).
    /// </summary>
    internal uint Color { get; set; }

    /// <summary>
    /// Glow intensity (0.0–1.0) used for pulsing effects.
    /// </summary>
    internal float Glow { get; set; }

    /// <summary>
    /// Whether node is pinned; pinned nodes are excluded from physics.
    /// </summary>
    internal bool Pinned { get; set; }

    /// <summary>
    /// Initializes new <see cref="SceneNode"/>.
    /// </summary>
    /// <param name="id">Unique node identifier.</param>
    /// <param name="kind">Node type.</param>
    /// <param name="position">Initial position.</param>
    /// <param name="mass">Physics mass (default 1f).</param>
    /// <param name="color">Initial color (default white).</param>
    internal SceneNode(string id, NodeKind kind, Vec2 position, float mass = 1f, uint color = 0xFFFFFF)
    {
        Id       = id;
        Kind     = kind;
        Position = position;
        Mass     = mass;
        Color    = color;
        Radius   = kind switch
        {
            NodeKind.Root   => 20f,
            NodeKind.Branch => 12f,
            _ => 6f    // File
        };
    }
}