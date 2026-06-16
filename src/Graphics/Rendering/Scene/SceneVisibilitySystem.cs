using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using System.Linq;

namespace ChangeTrace.Graphics.Rendering.Scene;

/// <summary>
/// Provides scene visibility helpers used by render passes.
/// </summary>
internal sealed class SceneVisibilitySystem
{
    private readonly HashSet<int> _visibleNodeIndices = [];
    private readonly List<EdgeSnapshotIndexed> _edgeBuffer = [];

    /// <summary>
    /// Builds hierarchy edges where both endpoints are visible.
    /// </summary>
    public IReadOnlyList<EdgeSnapshotIndexed> BuildVisibleHierarchyEdges(
        RenderState state,
        IReadOnlyList<NodeSnapshot> visibleNodes)
    {
        _visibleNodeIndices.Clear();
        _edgeBuffer.Clear();

        var visibleNodeIds = new HashSet<string>(visibleNodes.Select(node => node.Id));

        for (var i = 0; i < state.Scene.Nodes.Count; i++)
        {
            if (visibleNodeIds.Contains(state.Scene.Nodes[i].Id))
                _visibleNodeIndices.Add(i);
        }

        foreach (var edge in state.Scene.Edges)
        {
            if (edge.Kind != EdgeKind.Hierarchy)
                continue;

            if (!_visibleNodeIndices.Contains(edge.FromIndex) ||
                !_visibleNodeIndices.Contains(edge.ToIndex))
            {
                continue;
            }

            _edgeBuffer.Add(
                edge);
        }

        _visibleNodeIndices.Clear();

        return _edgeBuffer;
    }

    /// <summary>
    /// Splits nodes into branch/folder nodes and file nodes.
    /// </summary>
    public static void SplitNodes(
        IReadOnlyList<NodeSnapshot> nodes,
        ICollection<NodeSnapshot> branchNodes,
        ICollection<NodeSnapshot> fileNodes)
    {
        branchNodes.Clear();
        fileNodes.Clear();

        foreach (var node in nodes)
        {
            if (node.Kind == NodeKind.File)
            {
                fileNodes.Add(
                    node);
            }
            else
            {
                branchNodes.Add(
                    node);
            }
        }
    }
}
