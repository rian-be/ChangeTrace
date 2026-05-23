using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Layout.Hive.Clusters;
using ChangeTrace.Rendering.Layout.Hive.Geometry;
using ChangeTrace.Rendering.Layout.Hive.Rings;
using ChangeTrace.Rendering.Layout.Hive.Tree;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Core;

/// <summary>
/// Main hive layout engine responsible for directory placement,
/// heavy cluster layout, normalization, and animation.
/// </summary>
internal sealed class HiveLayout : ILayoutEngine
{
    private readonly HiveLayoutOptions _options = new();
    private readonly HiveTreeIndex _tree = new();

    private readonly HiveRingLayout _ringLayout;
    private readonly HiveClusterRegionLayout _clusterRegionLayout;
    private readonly HiveLayoutNormalizer _normalizer;
    private readonly HiveNodeAnimator _animator;

    private readonly List<HiveClusterInfo> _visibleClusters = [];

    private float _energy;

    /// <summary>
    /// Creates hive layout dependencies and supporting layout systems.
    /// </summary>
    public HiveLayout()
    {
        _ringLayout = new HiveRingLayout(_options);

        var labelBuilder = new HiveClusterLabelBuilder();

        var clusterBuilder = new HiveClusterBuilder(
            _options,
            labelBuilder,
            _ringLayout);

        _clusterRegionLayout = new HiveClusterRegionLayout(
            _options,
            clusterBuilder,
            _ringLayout);

        _normalizer = new HiveLayoutNormalizer(_options);
        _animator = new HiveNodeAnimator(_options);
    }

    /// <summary>
    /// Current layout energy used for convergence tracking.
    /// </summary>
    public float Energy => _energy;
    
    /// <summary>
    /// Rebuilds and animates the hive layout for the current frame.
    /// </summary>
    public void Step(
        IReadOnlyDictionary<string, SceneNode> nodes,
        float deltaSeconds)
    {
        _visibleClusters.Clear();

        if (nodes.Count == 0)
        {
            _energy = 0f;
            return;
        }

        _tree.Build(nodes, _options.RootId);

        var root = ResolveRoot(nodes);

        if (root is null)
        {
            _energy = 0f;
            return;
        }

        _tree.BuildWeights(_options.RootId, root.Id);

        PlaceRoot(root);
        var visualRoot = GetVisualRoot(root);

        LayoutRoot(root, visualRoot);
        _normalizer.Fit(nodes, _visibleClusters);
        _energy = _animator.AnimateToHome(nodes);
    }

    /// <summary>
    /// Finds the nearest visible heavy cluster at the given world position.
    /// </summary>
    public HiveClusterInfo? HitTestCluster(
        Vec2 worldPosition,
        float? padding = null)
    {
        HiveClusterInfo? best = null;
        var bestDistanceSq = float.MaxValue;

        var pickPadding = padding ?? _options.ClusterPickPadding;

        foreach (var cluster in _visibleClusters)
        {
            var delta = worldPosition - cluster.Center;
            var distanceSq = delta.LengthSq;
            var pickRadius = cluster.Radius + pickPadding;

            if (distanceSq > pickRadius * pickRadius)
                continue;

            if (distanceSq >= bestDistanceSq)
                continue;

            best = cluster;
            bestDistanceSq = distanceSq;
        }

        return best;
    }

    /// <summary>
    /// Resolves the logical root node used for layout traversal.
    /// </summary>
    private SceneNode? ResolveRoot(
        IReadOnlyDictionary<string, SceneNode> nodes)
    {
        if (nodes.TryGetValue(_options.RootId, out var root))
            return root;

        return nodes.Values.FirstOrDefault(x => x.Kind == NodeKind.Root);
    }

    /// <summary>
    /// Pins the root node to the world origin.
    /// </summary>
    private static void PlaceRoot(SceneNode root)
    {
        root.Pinned = true;
        root.Position = Vec2.Zero;
        root.HomePosition = Vec2.Zero;
        root.Velocity = Vec2.Zero;
        root.Force = Vec2.Zero;
    }

    /// <summary>
    /// Returns the effective visual root used for layout expansion.
    /// </summary>
    private SceneNode GetVisualRoot(SceneNode root)
    {
        if (!_tree.TryGetChildren(root.Id, out var rootChildren))
            return root;

        var directories = rootChildren
            .Where(x => x.Kind != NodeKind.File)
            .ToList();

        var files = rootChildren
            .Where(x => x.Kind == NodeKind.File)
            .ToList();

        if (directories.Count != 1 || files.Count != 0)
            return root;

        var container = directories[0];

        container.HomePosition = root.HomePosition;
        container.Position = root.HomePosition;
        container.Velocity = Vec2.Zero;
        container.Force = Vec2.Zero;

        return container;
    }

    /// <summary>
    /// Lays out the first level of the repository tree.
    /// </summary>
    private void LayoutRoot(
        SceneNode realRoot,
        SceneNode visualRoot)
    {
        if (!_tree.TryGetChildren(visualRoot.Id, out var children))
            return;

        var directories = HiveNodeSelector.GetDirectories(children);
        var files = HiveNodeSelector.GetFiles(children);

        if (files.Count > 0)
            LayoutFiles(realRoot, files);

        LayoutChildrenInSector(
            realRoot,
            directories,
            centerAngle: 0f,
            sector: MathF.Tau,
            depth: 1,
            minRadius: _options.MinRootRadius);
    }

    /// <summary>
    /// Lays out a directory subtree inside an angular sector.
    /// </summary>
    private void LayoutDirectory(
        SceneNode parent,
        float centerAngle,
        float sector,
        int depth)
    {
        if (!_tree.TryGetChildren(parent.Id, out var children))
            return;

        var directories = HiveNodeSelector.GetDirectories(children);
        var files = HiveNodeSelector.GetFiles(children);

        if (files.Count > 0)
            LayoutFiles(parent, files);

        if (directories.Count == 0)
            return;

        LayoutChildrenInSector(
            parent,
            directories,
            centerAngle,
            sector,
            depth,
            _options.MinChildRadius + depth * 120f);
    }

    /// <summary>
    /// Places sibling directories within an angular sector around a parent.
    /// </summary>
    private void LayoutChildrenInSector(
        SceneNode parent,
        IReadOnlyList<SceneNode> directories,
        float centerAngle,
        float sector,
        int depth,
        float minRadius)
    {
        if (directories.Count == 0)
            return;

        if (directories.Count == 1)
        {
            LayoutSingleDirectory(
                parent,
                directories[0],
                centerAngle,
                sector,
                depth,
                minRadius);

            return;
        }

        var radii = new float[directories.Count];
        var totalNeeded = 0f;

        for (var i = 0; i < directories.Count; i++)
        {
            var radius = EstimateDirectoryClusterRadius(directories[i]);

            radii[i] = radius;
            totalNeeded += radius * 2f + _options.SiblingGap;
        }

        var safeSector = Math.Clamp(sector, 0.35f, MathF.Tau);
        var radiusByArc = totalNeeded / safeSector;
        var radiusFromParent = MathF.Max(minRadius, radiusByArc);

        var spans = BuildAngularSpans(
            radii,
            radiusFromParent,
            safeSector,
            out var totalAngle);

        var cursor = centerAngle - totalAngle * 0.5f;

        for (var i = 0; i < directories.Count; i++)
        {
            var directory = directories[i];
            var span = spans[i];
            var angle = cursor + span * 0.5f;

            var radialOffset = MathF.Min(
                280f,
                MathF.Sqrt(_tree.GetWeight(directory.Id)) * 8f);

            var layerOffset = GetLayerOffset(i);

            var finalRadius = MathF.Max(
                minRadius,
                radiusFromParent + radialOffset + layerOffset);

            var target =
                parent.HomePosition +
                HiveGeometry.Direction(angle) * finalRadius;

            HiveGeometry.SetTarget(directory, target);

            var childSector = Math.Clamp(
                span * 1.35f,
                0.30f,
                MathF.PI * 0.78f);

            LayoutDirectory(
                directory,
                angle,
                childSector,
                depth + 1);

            cursor += span;
        }
    }

    /// <summary>
    /// Computes angular spans for sibling directory regions.
    /// </summary>
    private float[] BuildAngularSpans(
        IReadOnlyList<float> radii,
        float radiusFromParent,
        float safeSector,
        out float totalAngle)
    {
        var spans = new float[radii.Count];

        totalAngle = 0f;

        for (var i = 0; i < radii.Count; i++)
        {
            var span =
                (radii[i] * 2f + _options.SiblingGap) /
                radiusFromParent;

            spans[i] = span;
            totalAngle += span;
        }

        if (totalAngle <= safeSector)
            return spans;

        radiusFromParent *= totalAngle / safeSector;
        totalAngle = 0f;

        for (var i = 0; i < radii.Count; i++)
        {
            spans[i] =
                (radii[i] * 2f + _options.SiblingGap) /
                radiusFromParent;

            totalAngle += spans[i];
        }

        return spans;
    }

    /// <summary>
    /// Optimized layout path for a single child directory.
    /// </summary>
    private void LayoutSingleDirectory(
        SceneNode parent,
        SceneNode directory,
        float centerAngle,
        float sector,
        int depth,
        float minRadius)
    {
        var clusterRadius =
            minRadius +
            EstimateDirectoryClusterRadius(directory) * 1.10f;

        var target =
            parent.HomePosition +
            HiveGeometry.Direction(centerAngle) * clusterRadius;

        HiveGeometry.SetTarget(directory, target);

        LayoutDirectory(
            directory,
            centerAngle,
            Math.Clamp(
                sector * 0.68f,
                0.35f,
                MathF.PI * 0.95f),
            depth + 1);
    }

    /// <summary>
    /// Chooses between ring layout and heavy cluster layout for files.
    /// </summary>
    private void LayoutFiles(
        SceneNode parent,
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return;

        if (files.Count >= _options.HeavyFileFolderThreshold)
        {
            _clusterRegionLayout.Layout(
                parent,
                files,
                _visibleClusters);

            return;
        }

        _ringLayout.LayoutFiles(
            parent.HomePosition,
            parent.Id,
            files);
    }

    /// <summary>
    /// Estimates the visual radius occupied by a directory subtree.
    /// </summary>
    private float EstimateDirectoryClusterRadius(SceneNode node)
    {
        var fileCount = 0;
        var directoryCount = 0;

        if (_tree.TryGetChildren(node.Id, out var children))
        {
            foreach (var child in children)
            {
                if (child.Kind == NodeKind.File)
                    fileCount++;
                else
                    directoryCount++;
            }
        }

        var fileRadius =
            fileCount >= _options.HeavyFileFolderThreshold
                ? _clusterRegionLayout.EstimateHeavyFileClusterRadius(fileCount)
                : _ringLayout.EstimateFileShellRadius(fileCount);

        var directoryRadius =
            directoryCount > 0
                ? 160f + directoryCount * 54f
                : 0f;

        var weightRadius =
            MathF.Sqrt(_tree.GetWeight(node.Id)) * 10f;

        return Math.Clamp(
            MathF.Max(fileRadius, directoryRadius) + weightRadius,
            160f,
            1600f);
    }

    /// <summary>
    /// Adds deterministic radial variation between sibling layers.
    /// </summary>
    private static float GetLayerOffset(int index)
    {
        return (index % 4) switch
        {
            0 => -110f,
            1 => 20f,
            2 => 150f,
            _ => 250f
        };
    }
}