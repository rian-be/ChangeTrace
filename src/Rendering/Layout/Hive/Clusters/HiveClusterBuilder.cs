using ChangeTrace.Rendering.Layout.Hive.Core;
using ChangeTrace.Rendering.Layout.Hive.Geometry;
using ChangeTrace.Rendering.Layout.Hive.Rings;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Clusters;

/// <summary>
/// Builds clustered file groups for a hive layout.
/// </summary>
internal sealed class HiveClusterBuilder(
    HiveLayoutOptions options,
    HiveClusterLabelBuilder labelBuilder,
    HiveRingLayout ringLayout)
{
    /// <summary>
    /// Creates clusters for a parent node.
    /// </summary>
    public List<HiveCluster> Build(
        SceneNode parent,
        IReadOnlyList<SceneNode> files)
    {
        List<SceneNode> ordered =
            files
                .OrderBy(x =>
                    HiveGeometry.StableAngle(x.Id))
                .ToList();

        List<HiveCluster> clusters =
            [];

        int clusterIndex = 0;

        for (int i = 0;
             i < ordered.Count;
             i += options.HeavyFileClusterSize)
        {
            List<SceneNode> slice =
                ordered
                    .Skip(i)
                    .Take(options.HeavyFileClusterSize)
                    .ToList();

            clusters.Add(
                new HiveCluster
                {
                    Id = $"{parent.Id}::cluster::{clusterIndex}",
                    ParentId = parent.Id,
                    Label = labelBuilder.Build(
                        parent,
                        slice,
                        clusterIndex),
                    Files = slice,
                    ActivityScore = ComputeActivityScore(slice),
                    ImportanceScore = ComputeImportanceScore(slice),
                    Radius = EstimateClusterRadius(slice.Count)
                });

            clusterIndex++;
        }

        return clusters;
    }

    /// <summary>
    /// Estimates render radius for cluster size.
    /// </summary>
    public float EstimateClusterRadius(int fileCount)
    {
        if (fileCount <= 0)
            return options.FileRingStart;

        return ringLayout.EstimateFileShellRadius(fileCount);
    }

    /// <summary>
    /// Computes average activity score from a file glow.
    /// </summary>
    private float ComputeActivityScore(
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return 0f;

        float glowSum = 0f;

        foreach (SceneNode file in files)
            glowSum += file.Glow;

        return glowSum / files.Count;
    }

    /// <summary>
    /// Computes cluster importance score.
    /// </summary>
    private float ComputeImportanceScore(
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return 0f;

        float activity =
            ComputeActivityScore(files);

        float size =
            MathF.Sqrt(files.Count) *
            options.SizeImportanceWeight;

        return
            activity * options.GlowActivityWeight +
            size;
    }
}