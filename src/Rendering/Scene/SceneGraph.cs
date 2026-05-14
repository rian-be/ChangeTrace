using System.Numerics;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene.Relations;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Thread-safe mutable scene graph storing nodes, avatars, edges, and transient relations.
/// </summary>
/// <remarks>
/// Acts as the central runtime scene state used by layout, animation, snapshot assembly, and rendering systems.
/// Maintains cached edge views for hierarchy edges, transient edges, and bundled edges.
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class SceneGraph : ISceneGraph
{
    private readonly Lock _lock = new();

    private readonly Dictionary<string, SceneNode> _nodes = [];
    private readonly Dictionary<ActorName, ActorAvatar> _avatars = [];
    private readonly List<SceneEdge> _edges = [];
    private readonly List<SceneRelation> Relations = [];
    private readonly List<BundledEdge> _bundledEdges = [];

    /// <summary>
    /// Gets scene nodes indexed by node identifier.
    /// </summary>
    public IReadOnlyDictionary<string, SceneNode> Nodes
    {
        get { lock (_lock) return _nodes; }
    }

    /// <summary>
    /// Gets actor avatars indexed by actor name.
    /// </summary>
    public IReadOnlyDictionary<ActorName, ActorAvatar> Avatars
    {
        get { lock (_lock) return _avatars; }
    }

    private List<SceneEdge> _cachedEdges = [];
    private readonly List<SceneEdge> _hierarchyEdges = [];

    private bool _edgesDirty = true;
    private bool _hierarchyDirty = true;

    /// <summary>
    /// Gets cached lists of hierarchy, transient, and bundled scene edges.
    /// </summary>
    public IReadOnlyList<SceneEdge> Edges
    {
        get
        {
            lock (_lock)
            {
                if (!_edgesDirty && !_hierarchyDirty)
                    return _cachedEdges;

                if (_hierarchyDirty)
                {
                    _hierarchyEdges.Clear();

                    foreach (var node in _nodes.Values)
                    {
                        if (node.ParentId != null)
                        {
                            _hierarchyEdges.Add(
                                new SceneEdge(
                                    node.ParentId,
                                    node.Id,
                                    EdgeKind.Hierarchy,
                                    0));
                        }
                    }

                    _hierarchyDirty = false;
                }

                int totalCount = _hierarchyEdges.Count + _edges.Count;

                foreach (var b in _bundledEdges)
                    totalCount += b.Targets.Count;

                if (_cachedEdges.Capacity < totalCount)
                    _cachedEdges = new List<SceneEdge>(totalCount + 100);

                _cachedEdges.Clear();
                _cachedEdges.AddRange(_hierarchyEdges);
                _cachedEdges.AddRange(_edges);

                foreach (var bundled in _bundledEdges)
                {
                    var color = bundled.Color;

                    foreach (var targetId in bundled.Targets)
                    {
                        _cachedEdges.Add(
                            new SceneEdge(
                                bundled.FromId,
                                targetId,
                                bundled.Kind,
                                bundled.CreatedAt,
                                color));
                    }
                }

                _edgesDirty = false;
                return _cachedEdges;
            }
        }
    }

    /// <summary>
    /// Gets an existing scene node or creates a new one.
    /// </summary>
    /// <param name="id">Unique scene node identifier.</param>
    /// <param name="kind">Node kind used when creating a new node.</param>
    /// <param name="position">Initial node position.</param>
    /// <param name="color">Optional color override.</param>
    /// <returns>Existing or newly created scene node.</returns>
    public SceneNode GetOrAddNode(string id, NodeKind kind, Vec2 position, Vector4? color = null)
    {
        lock (_lock)
        {
            if (_nodes.TryGetValue(id, out var existing))
            {
                if (color.HasValue)
                    existing.Color = color.Value;

                return existing;
            }

            var node = new SceneNode(id, kind, position, color: color);
            _nodes[id] = node;

            if (node.ParentId != null && _nodes.TryGetValue(node.ParentId, out var parent))
                parent.IsParent = true;

            _edgesDirty = true;
            _hierarchyDirty = true;

            return node;
        }
    }

    /// <summary>
    /// Finds scene node by identifier.
    /// </summary>
    /// <param name="id">Scene node identifier.</param>
    /// <returns>Scene node or <c>null</c> when not found.</returns>
    public SceneNode? FindNode(string id)
    {
        lock (_lock)
            return _nodes.TryGetValue(id, out var node) ? node : null;
    }

    /// <summary>
    /// Removes scene node by identifier.
    /// </summary>
    /// <param name="id">Scene node identifier.</param>
    public void RemoveNode(string id)
    {
        lock (_lock)
        {
            _nodes.Remove(id);
            _edgesDirty = true;
            _hierarchyDirty = true;
        }
    }

    /// <summary>
    /// Gets an existing actor avatar or creates a new one.
    /// </summary>
    /// <param name="actor">Actor name.</param>
    /// <param name="spawnPos">Initial avatar position.</param>
    /// <param name="color">Avatar display color.</param>
    /// <returns>Existing or newly created actor avatar.</returns>
    public ActorAvatar GetOrAddAvatar(ActorName actor, Vec2 spawnPos, Vector4 color)
    {
        lock (_lock)
        {
            if (_avatars.TryGetValue(actor, out var existing))
                return existing;

            var avatar = new ActorAvatar(actor, spawnPos, color);
            _avatars[actor] = avatar;

            return avatar;
        }
    }

    /// <summary>
    /// Finds actor avatar by actor name.
    /// </summary>
    /// <param name="actor">Actor name.</param>
    /// <returns>Actor avatar or <c>null</c> when not found.</returns>
    public ActorAvatar? FindAvatar(ActorName actor)
    {
        lock (_lock)
            return _avatars.TryGetValue(actor, out var avatar) ? avatar : null;
    }

    /// <summary>
    /// Removes actor avatar by actor name.
    /// </summary>
    /// <param name="actor">Actor name.</param>
    public void RemoveAvatar(ActorName actor)
    {
        lock (_lock)
            _avatars.Remove(actor);
    }

    /// <summary>
    /// Removes all actor avatars from scenes.
    /// </summary>
    public void ClearAvatars()
    {
        lock (_lock)
            _avatars.Clear();
    }

    /// <summary>
    /// Adds transient scene edge.
    /// </summary>
    /// <param name="fromId">Source node identifier.</param>
    /// <param name="toId">Target node identifier.</param>
    /// <param name="kind">Edge kind.</param>
    /// <param name="virtualTime">Virtual timeline time when edge was created.</param>
    public void AddEdge(string fromId, string toId, EdgeKind kind, double virtualTime)
    {
        lock (_lock)
        {
            var edge = new SceneEdge(fromId, toId, kind, virtualTime) { Life = 0.6f };
            _edges.Add(edge);
            _edgesDirty = true;
        }
    }

    /// <summary>
    /// Adds transient bundled edge from one source to multiple target nodes.
    /// </summary>
    /// <param name="fromId">Source node identifier.</param>
    /// <param name="toIds">Target node identifiers.</param>
    /// <param name="kind">Edge kind.</param>
    /// <param name="virtualTime">Virtual timeline time when edge was created.</param>
    public void AddBundledEdge(string fromId, IEnumerable<string> toIds, EdgeKind kind, double virtualTime)
    {
        lock (_lock)
        {
            var toList = toIds.ToList();

            if (toList.Count == 0)
                return;

            var bundled = new BundledEdge(fromId, toList, kind, virtualTime) { Life = 0.6f };
            _bundledEdges.Add(bundled);
            _edgesDirty = true;
        }
    }

    /// <summary>
    /// Advances transient edge and relation lifetimes.
    /// </summary>
    /// <param name="dt">Delta time in seconds.</param>
    /// <param name="decayRate">Lifetime decay multiplier.</param>
    public void TickEdges(double dt, float decayRate)
    {
        lock (_lock)
        {
            bool changed = false;
            float fDt = (float)dt;

            for (int i = _edges.Count - 1; i >= 0; i--)
            {
                var edge = _edges[i];
                edge.Life -= fDt * decayRate;
                edge.Alpha = Math.Clamp(edge.Life / 0.5f, 0f, 1f);

                if (!(edge.Life <= 0)) continue;
                _edges.RemoveAt(i);
                changed = true;
            }

            for (int i = Relations.Count - 1; i >= 0; i--)
            {
                var relation = Relations[i];
                relation.Life -= fDt * decayRate;
                relation.Alpha = Math.Clamp(relation.Life / 0.5f, 0f, 1f);

                if (!(relation.Life <= 0)) continue;
                Relations.RemoveAt(i);
                changed = true;
            }

            for (int i = _bundledEdges.Count - 1; i >= 0; i--)
            {
                var bundled = _bundledEdges[i];
                bundled.Life -= fDt * decayRate;
                bundled.Alpha = Math.Clamp(bundled.Life / 0.5f, 0f, 1f);

                if (!(bundled.Life <= 0)) continue;
                _bundledEdges.RemoveAt(i);
                changed = true;
            }

            if (changed)
                _edgesDirty = true;
        }
    }

    /// <summary>
    /// Gets scene nodes matching the specified node kind.
    /// </summary>
    /// <param name="kind">Node kind to filter by.</param>
    /// <returns>Snapshot list of matching scene nodes.</returns>
    public IEnumerable<SceneNode> NodesOfKind(NodeKind kind)
    {
        lock (_lock)
            return _nodes.Values.Where(n => n.Kind == kind).ToList();
    }
        
    public void Clear()
    {
        lock (_lock)
        {
            _nodes.Clear();
            _avatars.Clear();
            _edges.Clear();
            Relations.Clear();
            _bundledEdges.Clear();

            _edgesDirty = true;
            _hierarchyDirty = true;
        }
    }
}