using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Scene.Relations;

namespace ChangeTrace.Rendering.States.Edges;

/// <summary>
/// Filters hierarchy edges for rendering based on visibility rules.
/// </summary>
internal sealed class EdgeVisibilityFilter
{
    private const int HeavyFileEdgeSiblingThreshold = 18;

    /// <summary>
    /// Counts file children grouped by parent node id.
    /// </summary>
    public Dictionary<string, int> CountFileChildrenByParent(
        ISceneGraph scene)
    {
        var counts =
            new Dictionary<string, int>();

        foreach (var node in scene.Nodes.Values)
        {
            if (node.Kind != NodeKind.File)
                continue;

            if (string.IsNullOrWhiteSpace(node.ParentId))
                continue;

            counts[node.ParentId] =
                counts.GetValueOrDefault(node.ParentId) + 1;
        }

        return counts;
    }

    /// <summary>
    /// Determines whether a hierarchy edge should be included in rendering.
    /// </summary>
    public bool ShouldInclude(
        SceneEdge edge,
        SceneNode fromNode,
        SceneNode toNode,
        IReadOnlyDictionary<string, int> fileChildrenByParent)
    {
        if (edge.Kind != EdgeKind.Hierarchy)
            return false;

        if (toNode.Kind != NodeKind.File)
            return true;

        var siblingFileCount =
            fileChildrenByParent.GetValueOrDefault(edge.FromId);

        return siblingFileCount < HeavyFileEdgeSiblingThreshold;
    }
}