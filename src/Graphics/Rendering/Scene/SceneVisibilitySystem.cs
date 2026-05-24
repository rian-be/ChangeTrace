using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Graphics.Rendering.Scene;

/// <summary>
/// Provides scene visibility helpers used by render passes.
/// </summary>
internal sealed class SceneVisibilitySystem
{
    private readonly HashSet<string> _visibleNodeIds = [];
    private readonly List<EdgeSnapshot> _edgeBuffer = [];

    /// <summary>
    /// Builds hierarchy edges where both endpoints are visible.
    /// </summary>
    public IReadOnlyList<EdgeSnapshot> BuildVisibleHierarchyEdges(
        RenderState state,
        IReadOnlyList<NodeSnapshot> visibleNodes)
    {
        _visibleNodeIds.Clear();
        _edgeBuffer.Clear();

        foreach (var node in visibleNodes)
        {
            _visibleNodeIds.Add(
                node.Id);
        }

        foreach (var edge in state.Scene.Edges)
        {
            if (edge.Kind != EdgeKind.Hierarchy)
                continue;

            if (!_visibleNodeIds.Contains(edge.FromId) ||
                !_visibleNodeIds.Contains(edge.ToId))
            {
                continue;
            }

            _edgeBuffer.Add(
                edge);
        }

        _visibleNodeIds.Clear();

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