// SceneGraph.cs
using System.Numerics;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene.Graph;
using ChangeTrace.Rendering.Scene.Relations;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Thread-safe facade for scene nodes, avatars, and runtime edges.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class SceneGraph : ISceneGraph
{
    private readonly Lock _lock = new();

    private readonly SceneNodeRegistry _nodes = new();
    private readonly SceneAvatarRegistry _avatars = new();
    private readonly SceneEdgeManager _edges = new();
    private readonly SceneHierarchyManager _hierarchy;

    /// <summary>
    /// Creates scene graph facade.
    /// </summary>
    public SceneGraph()
    {
        _hierarchy = new SceneHierarchyManager(_nodes);
    }

    /// <summary>
    /// Registered scene nodes.
    /// </summary>
    public IReadOnlyDictionary<string, SceneNode> Nodes
    {
        get { lock (_lock) return _nodes.Items; }
    }

    /// <summary>
    /// Registered actor avatars.
    /// </summary>
    public IReadOnlyDictionary<ActorName, ActorAvatar> Avatars
    {
        get { lock (_lock) return _avatars.Items; }
    }

    /// <summary>
    /// Combined hierarchy and runtime scene edges.
    /// </summary>
    public IReadOnlyList<SceneEdge> Edges
    {
        get { lock (_lock) return _edges.GetEdges(_nodes.Items, _hierarchy); }
    }

    /// <summary>
    /// Gets existing root node or creates it.
    /// </summary>
    public SceneNode GetOrCreateRoot()
    {
        lock (_lock)
            return _nodes.GetOrCreateRoot();
    }

    /// <summary>
    /// Gets existing node or creates a new one.
    /// </summary>
    public SceneNode GetOrAddNode(
        string id,
        NodeKind kind,
        Vec2 position,
        Vector4? color = null)
    {
        lock (_lock)
        {
            SceneNode node = _nodes.GetOrAddNode(
                id,
                kind,
                position,
                color,
                out bool topologyChanged);

            if (!topologyChanged)
                return node;

            _hierarchy.EnsureParentChain(node);
            _hierarchy.MarkDirty();
            _edges.MarkTopologyDirty();

            return node;
        }
    }

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    public SceneNode? FindNode(string id)
    {
        lock (_lock)
            return _nodes.Find(id);
    }

    /// <summary>
    /// Removes node by identifier.
    /// </summary>
    public void RemoveNode(string id)
    {
        lock (_lock)
        {
            if (!_nodes.Remove(id))
                return;

            _hierarchy.MarkDirty();
            _edges.MarkTopologyDirty();
        }
    }

    /// <summary>
    /// Gets existing avatar or creates a new one.
    /// </summary>
    public ActorAvatar GetOrAddAvatar(
        ActorName actor,
        Vec2 spawnPos,
        Vector4 color)
    {
        lock (_lock)
            return _avatars.GetOrAdd(actor, spawnPos, color);
    }

    /// <summary>
    /// Finds avatar by actor identifier.
    /// </summary>
    public ActorAvatar? FindAvatar(ActorName actor)
    {
        lock (_lock)
            return _avatars.Find(actor);
    }

    /// <summary>
    /// Removes avatar by actor identifier.
    /// </summary>
    public void RemoveAvatar(ActorName actor)
    {
        lock (_lock)
            _avatars.Remove(actor);
    }

    /// <summary>
    /// Removes all avatars.
    /// </summary>
    public void ClearAvatars()
    {
        lock (_lock)
            _avatars.Clear();
    }

    /// <summary>
    /// Adds runtime edge between two existing nodes.
    /// </summary>
    public void AddEdge(
        string fromId,
        string toId,
        EdgeKind kind,
        double virtualTime)
    {
        lock (_lock)
            _edges.AddEdge(_nodes.Items, fromId, toId, kind, virtualTime);
    }

    /// <summary>
    /// Adds bundled runtime edge from one node to multiple targets.
    /// </summary>
    public void AddBundledEdge(
        string fromId,
        IEnumerable<string> toIds,
        EdgeKind kind,
        double virtualTime)
    {
        lock (_lock)
            _edges.AddBundledEdge(_nodes.Items, fromId, toIds, kind, virtualTime);
    }

    /// <summary>
    /// Updates runtime edge lifetimes.
    /// </summary>
    public void TickEdges(double dt, float decayRate)
    {
        lock (_lock)
            _edges.Tick(dt, decayRate);
    }

    /// <summary>
    /// Returns nodes matching the specified kind.
    /// </summary>
    public IEnumerable<SceneNode> NodesOfKind(NodeKind kind)
    {
        lock (_lock)
            return _nodes.GetByKind(kind);
    }

    /// <summary>
    /// Clears all scene state.
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _nodes.Clear();
            _avatars.Clear();
            _edges.Clear();
            _hierarchy.Clear();
        }
    }
}