using ChangeTrace.Rendering.Layout.Proximity;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Forces;

/// <summary>
/// Force directed layout calculator using repulsion, spring attraction,
/// and global gravity forces.
/// </summary>
/// <remarks>
/// This implementation applies:
/// <list type="bullet">
/// <item><description>Pairwise repulsion between all nodes (O(n²)).</description></item>
/// <item><description>Spring forces between logically connected nodes.</description></item>
/// <item><description>Weak gravity pulling nodes toward the origin.</description></item>
/// </list>
/// The resulting force map is intended to be consumed by an <see cref="Integration.IIntegrator"/>.
/// </remarks>
internal sealed class ForceDirectedCalculator(INodeProximity proximity) : IForceCalculator
{
    /// <summary>
    /// Strength of pairwise repulsion force.
    /// Higher values increase node separation.
    /// </summary>
    public float RepulsionStrength { get; set; } = 8_000f;

    /// <summary>
    /// Spring stiffness for connected nodes.
    /// </summary>
    public float SpringStrength { get; set; } = 0.05f;

    /// <summary>
    /// Rest length of springs between connected nodes.
    /// </summary>
    public float SpringLength { get; set; } = 120f;

    /// <summary>
    /// Global gravity coefficient pulling nodes toward origin.
    /// </summary>
    public float Gravity { get; set; } = 0.02f;

    /// <summary>
    /// Calculates accumulated forces for all nodes in layout.
    /// </summary>
    /// <param name="nodes">Nodes participating in simulation.</param>
    /// <returns>
    /// Dictionary mapping node IDs to their computed force vectors.
    /// </returns>
    public Dictionary<string, Vec2> CalculateForces(IReadOnlyList<SceneNode> nodes)
    {
        var forces = new Dictionary<string, Vec2>();
        foreach (var node in nodes)
            forces[node.Id] = Vec2.Zero;

        // Pairwise repulsion (O(n²))
        for (var i = 0; i < nodes.Count; i++)
        for (var j = i + 1; j < nodes.Count; j++)
        {
            var a = nodes[i];
            var b = nodes[j];

            var delta = a.Position - b.Position;
            float distSq = delta.LengthSq + 0.01f;

            var force = delta.Normalized() * (RepulsionStrength / distSq);

            forces[a.Id] += force;
            forces[b.Id] -= force;

            // Spring force for connected nodes
            if (!proximity.AreConnected(a, b)) continue;
            
            var dist = delta.Length + 0.01f;
            var spring = delta.Normalized() *
                         (SpringStrength * (dist - SpringLength));

            forces[a.Id] += spring;
            forces[b.Id] -= spring;
        }

        // Global gravity toward origin
        foreach (var node in nodes)
            forces[node.Id] -= node.Position * Gravity;

        return forces;
    }
}