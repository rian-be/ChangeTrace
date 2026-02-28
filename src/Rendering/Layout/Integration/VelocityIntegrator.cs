using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Integration;

/// <summary>
/// Velocity based integrator using damped semi implicit Euler integration.
/// </summary>
/// <remarks>
/// Updates node velocity based on accumulated forces and applies damping to
/// stabilize simulation. Velocity is clamped to <see cref="MaxSpeed"/>
/// to prevent numerical instability or excessive movement.
/// Pinned nodes are excluded from integration.
/// </remarks>
internal sealed class VelocityIntegrator : IIntegrator
{
    /// <summary>
    /// Global damping factor applied to velocity each step.
    /// Values below 1 reduce oscillation and stabilize layout.
    /// </summary>
    public float Damping  { get; set; } = 0.85f;

    /// <summary>
    /// Maximum allowed node speed (units per second).
    /// </summary>
    public float MaxSpeed { get; set; } = 200f;

    /// <summary>
    /// Integrates forces and updates node velocity and position.
    /// </summary>
    /// <param name="nodes">Nodes participating in simulation step.</param>
    /// <param name="forces">Accumulated force vectors per node ID.</param>
    /// <param name="deltaSeconds">Elapsed simulation time in seconds.</param>
    public void Integrate(
        IReadOnlyList<SceneNode> nodes,
        Dictionary<string, Vec2> forces,
        float deltaSeconds)
    {
        foreach (var node in nodes)
        {
            if (node.Pinned) 
                continue;

            var acc= forces[node.Id] / node.Mass;
            var velocity = (node.Velocity + acc * deltaSeconds) * Damping;

            if (velocity.Length > MaxSpeed)
                velocity = velocity.Normalized() * MaxSpeed;

            node.Velocity  = velocity;
            node.Position += velocity * deltaSeconds;
        }
    }
}