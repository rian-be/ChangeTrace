using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Provides shared node flyweight instances by node kind.
/// </summary>
internal static class NodeFlyweightFactory
{
    private static readonly Dictionary<NodeKind, NodeFlyweight> Flyweights = new()
    {
        { NodeKind.Root,   new NodeFlyweight(NodeKind.Root,   18f, 10.0f) },
        { NodeKind.Branch, new NodeFlyweight(NodeKind.Branch, 8f,  5.0f) },
        { NodeKind.File,   new NodeFlyweight(NodeKind.File,   1.8f, 0.4f) }
    };

    /// <summary>
    /// Resolves flyweight for the specified node kind.
    /// </summary>
    public static NodeFlyweight ForKind(NodeKind kind)
    {
        return Flyweights.TryGetValue(kind, out var flyweight)
            ? flyweight
            : Flyweights[NodeKind.File];
    }
}