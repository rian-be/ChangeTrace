using ChangeTrace.Rendering.Layout.Hive.Core;
using ChangeTrace.Rendering.Layout.Hive.Geometry;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Rings;

/// <summary>
/// Places file nodes around a center point in deterministic concentric rings.
/// </summary>
internal sealed class HiveRingLayout(HiveLayoutOptions options)
{
    /// <summary>
    /// Lays out files around the center using stable ordering and light jitter.
    /// </summary>
    public void LayoutFiles(
        Vec2 center,
        string seed,
        IReadOnlyList<SceneNode> files)
    {
        var ordered = files
            .OrderBy(x => HiveGeometry.StableAngle(x.Id))
            .ToList();

        var placed = 0;
        var ring = 0;

        while (placed < ordered.Count)
        {
            var radius = options.FileRingStart + ring * options.FileRingGap;

            var capacityByCircumference = Math.Max(
                options.MinFilesPerRing,
                (int)((MathF.Tau * radius) / options.FileSpacing));

            var capacity = Math.Min(
                options.MaxFilesPerRing,
                capacityByCircumference);

            var remaining = ordered.Count - placed;
            var countOnRing = Math.Min(capacity, remaining);

            var start = HiveGeometry.Hash01(seed, 991 + ring) * MathF.Tau;
            start += ring * 0.37f;

            for (var i = 0; i < countOnRing; i++)
            {
                var file = ordered[placed++];

                var angle = start + i / (float)countOnRing * MathF.Tau;

                var jitterAngle =
                    (HiveGeometry.Hash01(file.Id, 17) - 0.5f) * 0.015f;

                var jitterRadius =
                    (HiveGeometry.Hash01(file.Id, 31) - 0.5f) * 2.0f;

                var target =
                    center +
                    HiveGeometry.Direction(angle + jitterAngle) *
                    (radius + jitterRadius);

                HiveGeometry.SetTarget(file, target);
            }

            ring++;
        }
    }

    /// <summary>
    /// Estimates the outer radius needed to contain the file rings.
    /// </summary>
    public float EstimateFileShellRadius(int fileCount)
    {
        if (fileCount <= 0)
            return options.FileRingStart + 48f;

        var rings = EstimateFileRingCount(fileCount);

        var lastRadius =
            options.FileRingStart +
            MathF.Max(0, rings - 1) *
            options.FileRingGap;

        return lastRadius + options.FileRingGap + 72f;
    }

    /// <summary>
    /// Estimates how many rings are needed for a file count.
    /// </summary>
    private int EstimateFileRingCount(int fileCount)
    {
        if (fileCount <= 0)
            return 0;

        var remaining = fileCount;
        var ring = 0;

        while (remaining > 0)
        {
            var radius = options.FileRingStart + ring * options.FileRingGap;

            var capacityByCircumference = Math.Max(
                options.MinFilesPerRing,
                (int)((MathF.Tau * radius) / options.FileSpacing));

            var capacity = Math.Min(
                options.MaxFilesPerRing,
                capacityByCircumference);

            remaining -= capacity;
            ring++;
        }

        return ring;
    }
}