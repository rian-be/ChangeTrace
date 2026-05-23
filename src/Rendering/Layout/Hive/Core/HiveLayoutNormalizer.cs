using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Layout.Hive.Clusters;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Core;

/// <summary>
/// Fits hive layout positions into the configured maximum radius.
/// </summary>
internal sealed class HiveLayoutNormalizer(HiveLayoutOptions options)
{
    private float _lastFitScale = 1.0f;

    /// <summary>
    /// Scales node home positions and visible clusters when the layout exceeds bounds.
    /// </summary>
    public void Fit(
        IReadOnlyDictionary<string, SceneNode> nodes,
        IReadOnlyList<HiveClusterInfo> visibleClusters)
    {
        var maxDistance = 0f;

        foreach (var node in nodes.Values)
        {
            if (node.Kind == NodeKind.Root)
                continue;

            var distance = MathF.Sqrt(node.HomePosition.LengthSq);

            if (distance > maxDistance)
                maxDistance = distance;
        }

        var scale = 1.0f;

        if (maxDistance > options.MaxLayoutRadius)
        {
            scale = options.MaxLayoutRadius / maxDistance;
            scale = MathF.Max(options.MinFitScale, scale);
        }

        foreach (var node in nodes.Values)
        {
            if (node.Kind == NodeKind.Root || node.Pinned)
                continue;

            node.HomePosition = new Vec2(
                node.HomePosition.X * scale,
                node.HomePosition.Y * scale);
        }

        if (MathF.Abs(scale - 1.0f) > 0.0001f)
        {
            foreach (var cluster in visibleClusters)
            {
                cluster.Center = new Vec2(
                    cluster.Center.X * scale,
                    cluster.Center.Y * scale);

                cluster.Radius *= scale;
            }
        }

        if (MathF.Abs(scale - _lastFitScale) > options.FitSnapThreshold)
            SnapNodesToHome(nodes);

        _lastFitScale = scale;
    }

    /// <summary>
    /// Moves unpinned nodes directly to their home positions.
    /// </summary>
    private static void SnapNodesToHome(
        IReadOnlyDictionary<string, SceneNode> nodes)
    {
        foreach (var node in nodes.Values)
        {
            if (node.Kind == NodeKind.Root || node.Pinned)
                continue;

            node.Position = node.HomePosition;
            node.Velocity = Vec2.Zero;
            node.Force = Vec2.Zero;
        }
    }
}