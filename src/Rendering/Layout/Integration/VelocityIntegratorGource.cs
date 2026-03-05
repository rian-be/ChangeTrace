using ChangeTrace.Rendering.Scene;
using System;
using System.Collections.Generic;

namespace ChangeTrace.Rendering.Layout.Integration;

/// <summary>
/// Integrator for Gource layout (velocity + damping)
/// </summary>
internal sealed class VelocityIntegratorGource : IIntegrator
{
    public float Damping { get; set; } = 0.85f;
    public float MaxSpeed { get; set; } = 200f;

    public void Integrate(IReadOnlyList<SceneNode> nodes, Dictionary<string, Vec2> forces, float deltaSeconds)
    {
        foreach (var node in nodes)
        {
            if (node.Pinned) continue;

            var acc = forces[node.Id] / node.Mass;
            var velocity = (node.Velocity + acc * deltaSeconds) * Damping;

            if (velocity.Length > MaxSpeed)
                velocity = velocity.Normalized() * MaxSpeed;

            node.Velocity = velocity;
            node.Position += velocity * deltaSeconds;
        }
    }
}