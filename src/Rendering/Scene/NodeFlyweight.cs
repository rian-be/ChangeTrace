using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Flyweight containing immutable shared node rendering and physics properties.
/// </summary>
/// <remarks>
/// Shared across scene nodes of the same <see cref="NodeKind"/> to reduce duplicated state.
/// </remarks>
internal sealed class NodeFlyweight
{
    /// <summary>
    /// Gets node kind associated with this flyweight.
    /// </summary>
    public NodeKind Kind { get; }

    /// <summary>
    /// Gets default render radius for nodes using this flyweight.
    /// </summary>
    public float Radius { get; }

    /// <summary>
    /// Gets default physics mass for nodes using this flyweight.
    /// </summary>
    public float Mass { get; }

    /// <summary>
    /// Creates node flyweight with shared rendering and physics parameters.
    /// </summary>
    /// <param name="kind">Associated node kind.</param>
    /// <param name="radius">Default render radius.</param>
    /// <param name="mass">Default physics mass.</param>
    internal NodeFlyweight(NodeKind kind, float radius, float mass)
    {
        Kind = kind;
        Radius = radius;
        Mass = mass;
    }
}