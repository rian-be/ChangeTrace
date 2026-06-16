using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Scene.Relations;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.States.Edges;

/// <summary>
/// Builds render snapshots for visible hierarchy edges.
/// </summary>
internal sealed class EdgeSnapshotAssembler
{
    private readonly EdgeVisibilityFilter _visibilityFilter = new();

    /// <summary>
    /// Converts visible scene edges into immutable render snapshots.
    /// </summary>
    public IReadOnlyList<EdgeSnapshotIndexed> Assemble(
        ISceneGraph scene,
        IReadOnlyDictionary<string, int> nodeIndex)
    {
        var sceneNodes = scene.Nodes;
        var edges = scene.Edges;
        var snapshots = new EdgeSnapshotIndexed[edges.Count];
        var snapshotCount = 0;
        var fileChildrenByParent = _visibilityFilter.CountFileChildrenByParent(sceneNodes);

        foreach (var edge in edges)
        {
            if (!TryResolveHierarchyEdge(
                    sceneNodes,
                    edge,
                    out var fromNode,
                    out var toNode))
            {
                continue;
            }

            if (!_visibilityFilter.ShouldInclude(
                    edge,
                    fromNode,
                    toNode,
                    fileChildrenByParent))
            {
                continue;
            }

            var style =
                EdgeSnapshotStyle.FromNodes(
                    fromNode,
                    toNode);

            if (!nodeIndex.TryGetValue(edge.FromId, out var fromIndex) ||
                !nodeIndex.TryGetValue(edge.ToId, out var toIndex))
            {
                continue;
            }

            snapshots[snapshotCount++] =
                new EdgeSnapshotIndexed(
                    fromIndex,
                    toIndex,
                    edge.Kind,
                    style.Alpha,
                    edge.Color,
                    style.ThicknessStart,
                    style.ThicknessEnd);
        }

        if (snapshotCount == snapshots.Length)
            return snapshots;

        var trimmed = new EdgeSnapshotIndexed[snapshotCount];
        Array.Copy(snapshots, trimmed, snapshotCount);
        return trimmed;
    }

    /// <summary>
    /// Resolves hierarchy edge endpoints from the scene graph.
    /// </summary>
    private static bool TryResolveHierarchyEdge(
        IReadOnlyDictionary<string, SceneNode> nodes,
        SceneEdge edge,
        out SceneNode fromNode,
        out SceneNode toNode)
    {
        fromNode = null!;
        toNode = null!;

        if (edge.Kind != EdgeKind.Hierarchy)
            return false;

        if (!nodes.TryGetValue(edge.FromId, out var resolvedFrom))
            return false;

        if (!nodes.TryGetValue(edge.ToId, out var resolvedTo))
            return false;

        fromNode = resolvedFrom;
        toNode = resolvedTo;

        return true;
    }

    /// <summary>
    /// Visual styling parameters derived from edge endpoint types.
    /// </summary>
    private readonly record struct EdgeSnapshotStyle(
        float Alpha,
        float ThicknessStart,
        float ThicknessEnd)
    {
        /// <summary>
        /// Creates edge styling for the given node pair.
        /// </summary>
        public static EdgeSnapshotStyle FromNodes(
            SceneNode fromNode,
            SceneNode toNode)
        {
            var isToFile =
                toNode.Kind == NodeKind.File;

            var isRootEdge =
                fromNode.Kind == NodeKind.Root ||
                fromNode.Id == SceneIds.Root;

            var alpha = 0.10f;
            var thicknessStart = 0.55f;
            var thicknessEnd = 1.25f;

            if (isToFile)
            {
                alpha = 0.025f;
                thicknessStart = 0.10f;
                thicknessEnd = 0.16f;
            }

            if (isRootEdge && !isToFile)
                alpha = 0.18f;

            return new EdgeSnapshotStyle(
                alpha,
                thicknessStart,
                thicknessEnd);
        }
    }
}
