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
    public List<EdgeSnapshot> Assemble(
        ISceneGraph scene)
    {
        var snapshots =
            new List<EdgeSnapshot>(
                scene.Edges.Count);

        var fileChildrenByParent =
            _visibilityFilter.CountFileChildrenByParent(scene);

        foreach (var edge in scene.Edges)
        {
            if (!TryResolveHierarchyEdge(
                    scene,
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

            snapshots.Add(
                new EdgeSnapshot(
                    edge.FromId,
                    edge.ToId,
                    edge.Kind,
                    style.Alpha,
                    edge.Color,
                    style.ThicknessStart,
                    style.ThicknessEnd));
        }

        return snapshots;
    }

    /// <summary>
    /// Resolves hierarchy edge endpoints from the scene graph.
    /// </summary>
    private static bool TryResolveHierarchyEdge(
        ISceneGraph scene,
        SceneEdge edge,
        out SceneNode fromNode,
        out SceneNode toNode)
    {
        fromNode = null!;
        toNode = null!;

        if (edge.Kind != EdgeKind.Hierarchy)
            return false;

        var resolvedFrom =
            scene.FindNode(edge.FromId);

        if (resolvedFrom is null)
            return false;

        var resolvedTo =
            scene.FindNode(edge.ToId);

        if (resolvedTo is null)
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