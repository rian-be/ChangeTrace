using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// An in memory scene graph for visualization purposes.
/// </summary>
/// <remarks>
/// Holds current state of scene, including nodes (<see cref="SceneNode"/>),
/// avatars (<see cref="ActorAvatar"/>), and edges (<see cref="SceneEdge"/>).
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class SceneGraph : ISceneGraph
{
    private readonly Dictionary<string, SceneNode> _nodes = new();
    private readonly Dictionary<ActorName, ActorAvatar> _avatars = new();
    private readonly List<SceneEdge> _edges = [];

    private const float EdgeLifetimeSeconds = 4f;

    public IReadOnlyDictionary<string, SceneNode> Nodes => _nodes;
    public IReadOnlyDictionary<ActorName, ActorAvatar> Avatars => _avatars;
    public IReadOnlyList<SceneEdge> Edges => _edges;
    
    /// <summary>
    /// Gets existing node by identifier or creates a new one if it does not exist.
    /// </summary>
    /// <param name="id">Unique node identifier.</param>
    /// <param name="kind">Logical node kind.</param>
    /// <param name="position">Initial node position in scene space.</param>
    /// <param name="color">Packed RGB color value.</param>
    /// <returns>Existing or newly created <see cref="SceneNode"/>.</returns>
    public SceneNode GetOrAddNode(string id, NodeKind kind, Vec2 position, uint color = 0xAAAAAA)
    {
        if (_nodes.TryGetValue(id, out var existing))
            return existing;

        var node = new SceneNode(id, kind, position, color);
        _nodes.Add(id, node);
        return node;
    }

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    /// <param name="id">Node identifier.</param>
    /// <returns>
    /// Matching <see cref="SceneNode"/> if found; otherwise <c>null</c>.
    /// </returns>
    public SceneNode? FindNode(string id)
        => _nodes.TryGetValue(id, out var node) ? node : null;

    /// <summary>
    /// Removes node with specified identifier.
    /// </summary>
    /// <param name="id">Node identifier.</param>
    public void RemoveNode(string id)
        => _nodes.Remove(id);

    /// <summary>
    /// Gets existing avatar for actor or creates a new one.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <param name="spawnPos">Initial avatar position.</param>
    /// <param name="color">Packed RGB color value.</param>
    /// <returns>Existing or newly created <see cref="ActorAvatar"/>.</returns>
    public ActorAvatar GetOrAddAvatar(ActorName actor, Vec2 spawnPos, uint color)
    {
        if (_avatars.TryGetValue(actor, out var existing))
            return existing;

        var avatar = new ActorAvatar(actor, spawnPos, color);
        _avatars.Add(actor, avatar);
        return avatar;
    }

    /// <summary>
    /// Finds avatar by actor identifier.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <returns>
    /// Matching <see cref="ActorAvatar"/> if found; otherwise <c>null</c>.
    /// </returns>
    public ActorAvatar? FindAvatar(ActorName actor)
        => _avatars.TryGetValue(actor, out var avatar) ? avatar : null;

    /// <summary>
    /// Adds a new edge between two nodes.
    /// </summary>
    /// <param name="fromId">Source node identifier.</param>
    /// <param name="toId">Target node identifier.</param>
    /// <param name="kind">Edge classification.</param>
    /// <param name="virtualTime">Logical time of edge creation.</param>
    public void AddEdge(string fromId, string toId, EdgeKind kind, double virtualTime)
        => _edges.Add(new SceneEdge(fromId, toId, kind, virtualTime));

    /// <summary>
    /// Updates edge alpha values and removes expired edges.
    /// </summary>
    /// <param name="virtualTime">Current logical time.</param>
    /// <param name="decayRate">Linear decay multiplier applied to normalized lifetime.</param>
    /// <remarks>
    /// Edge lifetime is normalized using internal constant duration
    /// (<c>EdgeLifetimeSeconds</c>). Alpha fades linearly towards zero.
    /// </remarks>
    public void TickEdges(double virtualTime, float decayRate)
    {
        for (var i = _edges.Count - 1; i >= 0; i--)
        {
            var edge = _edges[i];

            var age = virtualTime - edge.CreatedAt;
            var t = (float)(age / EdgeLifetimeSeconds);

            edge.Alpha = Math.Clamp(1f - t * decayRate, 0f, 1f);

            if (edge.Alpha == 0f)
                _edges.RemoveAt(i);
        }
    }

    /// <summary>
    /// Enumerates nodes of specified kind.
    /// </summary>
    /// <param name="kind">Node kind to filter by.</param>
    /// <returns>
    /// Sequence of <see cref="SceneNode"/> instances matching specified kind.
    /// </returns>
    public IEnumerable<SceneNode> NodesOfKind(NodeKind kind)
    {
        foreach (var node in _nodes.Values)
        {
            if (node.Kind == kind)
                yield return node;
        }
    }

    /// <summary>
    /// Removes all nodes, avatars, and edges from scene graph.
    /// </summary>
    public void Clear()
    {
        _nodes.Clear();
        _avatars.Clear();
        _edges.Clear();
    }
}