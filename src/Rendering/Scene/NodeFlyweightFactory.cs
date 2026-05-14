using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Factory and registry for shared <see cref="NodeFlyweight"/> instances.
/// </summary>
/// <remarks>
/// Ensures a single immutable flyweight exists for each supported <see cref="NodeKind"/>.
/// </remarks>
internal static class NodeFlyweightFactory
{
    private static readonly Dictionary<NodeKind, NodeFlyweight> Flyweights = new()
    {
        { NodeKind.Root,   new NodeFlyweight(NodeKind.Root,   14f, 10.0f) },
        { NodeKind.Branch, new NodeFlyweight(NodeKind.Branch, 10f, 5.0f) },
        { NodeKind.File,   new NodeFlyweight(NodeKind.File,   6f,  1.0f) }
    };

    /// <summary>
    /// Gets a shared flyweight instance for a specified node kind.
    /// </summary>
    /// <param name="kind">Node kind.</param>
    /// <returns>Shared flyweight instance.</returns>
    public static NodeFlyweight ForKind(NodeKind kind)
    {
        return Flyweights.TryGetValue(kind, out var flyweight)
            ? flyweight
            : Flyweights[NodeKind.File];
    }
}