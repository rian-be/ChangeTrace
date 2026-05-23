// Graph/SceneHierarchyManager.cs
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene.Relations;

namespace ChangeTrace.Rendering.Scene.Graph;

/// <summary>
/// Maintains node hierarchy and cached hierarchy edges.
/// </summary>
internal sealed class SceneHierarchyManager
{
    /// <summary>
    /// Scene node registry.
    /// </summary>
    private readonly SceneNodeRegistry _nodes;

    /// <summary>
    /// Cached hierarchy edges.
    /// </summary>
    private readonly List<SceneEdge> _edges = [];

    /// <summary>
    /// Temporary cycle detection set.
    /// </summary>
    private readonly HashSet<string> _visited = [];

    /// <summary>
    /// Deduplicates generated hierarchy edges.
    /// </summary>
    private readonly HashSet<(string From, string To)> _seenEdges = [];

    /// <summary>
    /// Indicates hierarchy cache invalidation.
    /// </summary>
    private bool _dirty = true;

    /// <summary>
    /// Current hierarchy version.
    /// </summary>
    private int _version;

    /// <summary>
    /// Creates hierarchy manager.
    /// </summary>
    public SceneHierarchyManager(SceneNodeRegistry nodes)
    {
        _nodes = nodes;
    }

    /// <summary>
    /// Current hierarchy version.
    /// </summary>
    public int Version => _version;

    /// <summary>
    /// Cached hierarchy edges.
    /// </summary>
    public IReadOnlyList<SceneEdge> Edges
    {
        get
        {
            RebuildIfDirty();
            return _edges;
        }
    }

    /// <summary>
    /// Marks hierarchy cache as dirty.
    /// </summary>
    public void MarkDirty()
    {
        _dirty = true;
        _version++;
    }

    /// <summary>
    /// Ensures that all parent nodes exist.
    /// </summary>
    public void EnsureParentChain(SceneNode node)
    {
        if (node.Kind == NodeKind.Root)
            return;

        if (string.IsNullOrWhiteSpace(node.ParentId) ||
            node.ParentId == node.Id)
        {
            node.ParentId = SceneIds.Root;
        }

        string parentId = node.ParentId;

        if (string.IsNullOrWhiteSpace(parentId) ||
            parentId == SceneIds.Root)
        {
            node.ParentId = SceneIds.Root;
            _nodes.GetOrCreateRoot().IsParent = true;
            return;
        }

        _visited.Clear();

        while (true)
        {
            if (string.IsNullOrWhiteSpace(parentId) ||
                parentId == SceneIds.Root)
            {
                _nodes.GetOrCreateRoot().IsParent = true;
                return;
            }

            if (!_visited.Add(parentId))
            {
                node.ParentId = SceneIds.Root;
                _nodes.GetOrCreateRoot().IsParent = true;
                return;
            }

            SceneNode parent = GetOrCreateParent(
                parentId,
                node.Position);

            parent.IsParent = true;

            if (string.IsNullOrWhiteSpace(parent.ParentId) ||
                parent.ParentId == parent.Id)
            {
                parent.ParentId = SceneIds.Root;
                _nodes.GetOrCreateRoot().IsParent = true;
                return;
            }

            parentId = parent.ParentId;
        }
    }

    /// <summary>
    /// Clears hierarchy cache state.
    /// </summary>
    public void Clear()
    {
        _edges.Clear();
        _visited.Clear();
        _seenEdges.Clear();

        _dirty = true;
        _version++;
    }

    /// <summary>
    /// Gets existing parent node or creates a branch node.
    /// </summary>
    private SceneNode GetOrCreateParent(
        string id,
        Vec2 fallbackPosition)
    {
        if (_nodes.TryGet(id, out SceneNode parent))
            return parent;

        parent = new SceneNode(
            id,
            NodeKind.Branch,
            fallbackPosition);

        _nodes.Add(parent);

        return parent;
    }

    /// <summary>
    /// Rebuilds hierarchy edge cache if needed.
    /// </summary>
    private void RebuildIfDirty()
    {
        if (!_dirty)
            return;

        _edges.Clear();
        _seenEdges.Clear();

        foreach (SceneNode node in _nodes.Items.Values)
        {
            if (node.Kind == NodeKind.Root)
                continue;

            if (string.IsNullOrWhiteSpace(node.ParentId) ||
                node.ParentId == node.Id)
            {
                node.ParentId = SceneIds.Root;
            }

            if (!_nodes.Contains(node.ParentId))
                continue;

            var key = (node.ParentId, node.Id);

            if (!_seenEdges.Add(key))
                continue;

            _edges.Add(new SceneEdge(
                node.ParentId,
                node.Id,
                EdgeKind.Hierarchy,
                0));
        }

        _dirty = false;
    }
}