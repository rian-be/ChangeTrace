using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Tree;

/// <summary>
/// Builds parent-child lookup data and subtree weights for hive layout.
/// </summary>
internal sealed class HiveTreeIndex
{
    private readonly Dictionary<string, List<SceneNode>> _children = new();
    private readonly Dictionary<string, int> _weights = new();
    private readonly HashSet<string> _visited = [];

    /// <summary>
    /// Child nodes grouped by parent node id.
    /// </summary>
    public IReadOnlyDictionary<string, List<SceneNode>> Children => _children;

    /// <summary>
    /// Rebuilds the parent-child index from scene nodes.
    /// </summary>
    public void Build(
        IReadOnlyDictionary<string, SceneNode> nodes,
        string rootId)
    {
        _children.Clear();

        foreach (var node in nodes.Values)
        {
            node.Force = Vec2.Zero;

            if (node.Kind == NodeKind.Root || node.Id == rootId)
                continue;

            var parentId = string.IsNullOrWhiteSpace(node.ParentId)
                ? rootId
                : node.ParentId;

            node.ParentId = parentId;

            if (!_children.TryGetValue(parentId, out var list))
            {
                list = [];
                _children[parentId] = list;
            }

            list.Add(node);
        }
    }

    /// <summary>
    /// Recomputes subtree weights from the synthetic and real roots.
    /// </summary>
    public void BuildWeights(string rootId, string realRootId)
    {
        _weights.Clear();
        _visited.Clear();

        Weight(rootId);
        Weight(realRootId);

        foreach (var parentId in _children.Keys)
            Weight(parentId);
    }

    /// <summary>
    /// Returns the cached subtree weight for a node.
    /// </summary>
    public int GetWeight(string id)
    {
        return _weights.GetValueOrDefault(id, 1);
    }

    /// <summary>
    /// Gets indexed children for a node if any exist.
    /// </summary>
    public bool TryGetChildren(string id, out List<SceneNode> children)
    {
        return _children.TryGetValue(id, out children!);
    }

    /// <summary>
    /// Recursively computes subtree weight with cycle protection.
    /// </summary>
    private int Weight(string id)
    {
        if (!_visited.Add(id))
            return _weights.GetValueOrDefault(id, 1);

        var weight = 1;

        if (_children.TryGetValue(id, out var children))
        {
            foreach (var child in children)
                weight += Weight(child.Id);
        }

        _weights[id] = weight;

        return weight;
    }
}