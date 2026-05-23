using System.Numerics;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene.Graph;

/// <summary>
/// Stores and manages scene nodes.
/// </summary>
internal sealed class SceneNodeRegistry
{
    /// <summary>
    /// Scene nodes indexed by identifier.
    /// </summary>
    private readonly Dictionary<string, SceneNode> _nodes =
        [];

    private int _rootFileCount;

    /// <summary>
    /// Registered scene nodes.
    /// </summary>
    public IReadOnlyDictionary<string, SceneNode> Items =>
        _nodes;

    /// <summary>
    /// Gets an existing root node or creates it.
    /// </summary>
    public SceneNode GetOrCreateRoot()
    {
        if (_nodes.TryGetValue(
                SceneIds.Root,
                out SceneNode? existing))
        {
            return existing;
        }

        SceneNode root =
            new SceneNode(
                SceneIds.Root,
                NodeKind.Root,
                Vec2.Zero)
            {
                Pinned = true,
                Position = Vec2.Zero,
                HomePosition = Vec2.Zero,
                Velocity = Vec2.Zero
            };

        _nodes[SceneIds.Root] =
            root;

        return root;
    }

    /// <summary>
    /// Gets an existing node or creates a new one.
    /// </summary>
    public SceneNode GetOrAddNode(
        string id,
        NodeKind kind,
        Vec2 position,
        Vector4? color,
        out bool topologyChanged)
    {
        topologyChanged = false;

        id =
            SceneNodeIds.Normalize(
                id,
                kind);

        if (kind == NodeKind.Root ||
            id == SceneIds.Root)
        {
            return GetOrCreateRoot();
        }

        if (_nodes.TryGetValue(
                id,
                out SceneNode? existing))
        {
            if (color.HasValue)
                existing.Color = color.Value;

            if (!string.IsNullOrWhiteSpace(existing.ParentId) ||
                existing.Kind == NodeKind.Root)
            {
                return existing;
            }

            existing.ParentId =
                SceneNodeIds.ResolveParentId(
                    id,
                    existing.Kind);

            topologyChanged = true;

            return existing;
        }

        SceneNode node =
            CreateNode(
                id,
                kind,
                position,
                color);

        _nodes[node.Id] =
            node;

        topologyChanged = true;

        return node;
    }

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    public SceneNode? Find(string id)
    {
        id =
            SceneNodeIds.Normalize(
                id,
                NodeKind.File);

        return _nodes.TryGetValue(
            id,
            out SceneNode? node)
            ? node
            : null;
    }

    /// <summary>
    /// Removes node by identifier.
    /// </summary>
    public bool Remove(string id)
    {
        id =
            SceneNodeIds.Normalize(
                id,
                NodeKind.File);

        if (id == SceneIds.Root)
            return false;

        if (!_nodes.Remove(
                id,
                out SceneNode? removed))
        {
            return false;
        }

        if (removed.Kind == NodeKind.File &&
            removed.ParentId == SceneIds.Root &&
            _rootFileCount > 0)
        {
            _rootFileCount--;
        }

        return true;
    }

    /// <summary>
    /// Checks whether a node exists.
    /// </summary>
    public bool Contains(string id) =>
        _nodes.ContainsKey(id);

    /// <summary>
    /// Tries to get node by identifier.
    /// </summary>
    public bool TryGet(
        string id,
        out SceneNode node)
    {
        return _nodes.TryGetValue(
            id,
            out node!);
    }

    /// <summary>
    /// Adds or replaces scene node.
    /// </summary>
    public void Add(SceneNode node)
    {
        _nodes[node.Id] =
            node;
    }

    /// <summary>
    /// Returns nodes matching the specified kind.
    /// </summary>
    public List<SceneNode> GetByKind(NodeKind kind)
    {
        List<SceneNode> result =
            [];

        foreach (SceneNode node in _nodes.Values)
        {
            if (node.Kind == kind)
                result.Add(node);
        }

        return result;
    }

    /// <summary>
    /// Clears all registered nodes.
    /// </summary>
    public void Clear()
    {
        _nodes.Clear();
        _rootFileCount = 0;
    }

    /// <summary>
    /// Creates a scene node with initial placement.
    /// </summary>
    private SceneNode CreateNode(
        string id,
        NodeKind kind,
        Vec2 position,
        Vector4? color)
    {
        SceneNode node =
            new SceneNode(
                id,
                kind,
                position,
                color: color);

        if (node.Kind != NodeKind.File ||
            node.ParentId != SceneIds.Root)
        {
            return node;
        }

        int index =
            _rootFileCount++;

        float angle =
            index * MathF.Tau / 8f;

        const float radius = 260f;

        node.Position =
            new Vec2(
                MathF.Cos(angle) * radius,
                MathF.Sin(angle) * radius);

        node.HomePosition =
            node.Position;

        node.Velocity =
            Vec2.Zero;

        return node;
    }
}