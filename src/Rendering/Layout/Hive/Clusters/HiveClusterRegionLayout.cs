using ChangeTrace.Rendering.Layout.Hive.Core;
using ChangeTrace.Rendering.Layout.Hive.Geometry;
using ChangeTrace.Rendering.Layout.Hive.Rings;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Clusters;

/// <summary>
/// Lays out heavy file clusters inside an outer region around a parent node.
/// </summary>
internal sealed class HiveClusterRegionLayout(
    HiveLayoutOptions options,
    HiveClusterBuilder clusterBuilder,
    HiveRingLayout ringLayout)
{
    /// <summary>
    /// Builds clusters and places them in a centered staggered grid region.
    /// </summary>
    public void Layout(
        SceneNode parent,
        IReadOnlyList<SceneNode> files,
        ICollection<HiveClusterInfo> visibleClusters)
    {
        var clusters = clusterBuilder.Build(parent, files);

        if (clusters.Count == 0)
            return;

        var columns = EstimateHeavyColumns(clusters.Count);
        var rows = Math.Max(1, (clusters.Count + columns - 1) / columns);

        var outwardAngle =
            parent.HomePosition.LengthSq > 1f
                ? MathF.Atan2(parent.HomePosition.Y, parent.HomePosition.X)
                : HiveGeometry.StableAngle(parent.Id);

        var angleOffset =
            (HiveGeometry.Hash01(parent.Id, 4545) - 0.5f) *
            MathF.PI *
            0.18f;

        var regionDistance =
            options.HeavyRegionDistance +
            EstimateHeavyRegionRadius(clusters.Count) * 0.42f;

        var regionCenter =
            parent.HomePosition +
            HiveGeometry.Direction(outwardAngle + angleOffset) *
            regionDistance;

        var axisX = HiveGeometry.Direction(outwardAngle + angleOffset + MathF.PI * 0.5f);
        var axisY = HiveGeometry.Direction(outwardAngle + angleOffset);

        var orderedClusters = clusters
            .OrderByDescending(x => x.ImportanceScore)
            .ThenByDescending(x => x.ActivityScore)
            .ThenBy(x => HiveGeometry.StableAngle(x.Id))
            .ToList();

        for (var i = 0; i < orderedClusters.Count; i++)
        {
            var cluster = orderedClusters[i];

            var grid = IndexToCenteredGrid(
                i,
                columns,
                rows,
                orderedClusters.Count);

            var jitterX =
                (HiveGeometry.Hash01(cluster.Id + "::jx", 812) - 0.5f) *
                2f *
                options.HeavyClusterJitter;

            var jitterY =
                (HiveGeometry.Hash01(cluster.Id + "::jy", 913) - 0.5f) *
                2f *
                options.HeavyClusterJitter;

            cluster.Center =
                regionCenter +
                axisX * (grid.X + jitterX) +
                axisY * (grid.Y + jitterY);

            cluster.Radius = clusterBuilder.EstimateClusterRadius(cluster.Files.Count);

            ringLayout.LayoutFiles(
                cluster.Center,
                cluster.Id,
                cluster.Files);

            visibleClusters.Add(cluster.ToInfo());
        }
    }

    /// <summary>
    /// Estimates the total visual radius required for a heavy file region.
    /// </summary>
    public float EstimateHeavyFileClusterRadius(int fileCount)
    {
        if (fileCount <= 0)
            return 180f;

        var clusterCount =
            (fileCount + options.HeavyFileClusterSize - 1) /
            options.HeavyFileClusterSize;

        return EstimateHeavyRegionRadius(clusterCount) + 320f;
    }

    /// <summary>
    /// Estimates the radius occupied by the cluster grid.
    /// </summary>
    private float EstimateHeavyRegionRadius(int clusterCount)
    {
        if (clusterCount <= 0)
            return 180f;

        var columns = EstimateHeavyColumns(clusterCount);
        var rows = Math.Max(1, (clusterCount + columns - 1) / columns);

        var width =
            Math.Max(1, columns - 1) *
            options.HeavyClusterSpacingX;

        var height =
            Math.Max(1, rows - 1) *
            options.HeavyClusterSpacingY;

        return
            MathF.Sqrt(width * width + height * height) * 0.5f +
            options.FileRingStart +
            options.FileRingGap * 2f;
    }

    /// <summary>
    /// Estimates the number of columns used by the cluster grid.
    /// </summary>
    private int EstimateHeavyColumns(int clusterCount)
    {
        if (clusterCount <= 0)
            return options.HeavyMinColumns;

        var columns = (int)MathF.Ceiling(
            MathF.Sqrt(clusterCount * 1.25f));

        return Math.Clamp(
            columns,
            options.HeavyMinColumns,
            options.HeavyMaxColumns);
    }

    /// <summary>
    /// Converts a linear index into a centered staggered grid position.
    /// </summary>
    private Vec2 IndexToCenteredGrid(
        int index,
        int columns,
        int rows,
        int total)
    {
        var row = index / columns;
        var column = index % columns;

        var clustersInThisRow = Math.Min(
            columns,
            total - row * columns);

        var rowWidth =
            (clustersInThisRow - 1) *
            options.HeavyClusterSpacingX;

        var x =
            column * options.HeavyClusterSpacingX -
            rowWidth * 0.5f;

        var y =
            row * options.HeavyClusterSpacingY -
            (rows - 1) *
            options.HeavyClusterSpacingY *
            0.5f;

        if ((row & 1) == 1)
            x += options.HeavyClusterSpacingX * 0.5f;

        return new Vec2(x, y);
    }
}