using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout;

internal sealed class HiveLayout : ILayoutEngine
{
    private const string RootId = "__repo_root__";

    private const float AnimSmooth = 0.28f;

    /*
     * Globalny limit layoutu.
     */
    private const float MaxLayoutRadius = 4200f;
    private const float MinFitScale = 0.02f;
    private const float FitSnapThreshold = 0.08f;

    private const float MinRootRadius = 620f;
    private const float MinChildRadius = 420f;

    private const float SiblingGap = 180f;

    /*
     * Normalne pliki.
     */
    private const float FileSpacing = 68f;
    private const float FileRingStart = 110f;
    private const float FileRingGap = 88f;

    private const int MinFilesPerRing = 5;
    private const int MaxFilesPerRing = 10;

    /*
     * Folder z większą liczbą plików.
     *
     * 300 było zbyt wysoko: foldery średnie dalej robiły wachlarze / długie pasma.
     * 120 jest bezpiecznym startem. Jak nadal będzie źle, można zejść do 80.
     */
    private const int HeavyFileFolderThreshold = 120;

    /*
     * Ile plików w jednym podzie.
     */
    private const int HeavyFilePodSize = 18;

    /*
     * Odstęp między podami.
     */
    private const float HeavyPodSpacingX = 560f;
    private const float HeavyPodSpacingY = 520f;

    /*
     * Oddalenie całego regionu podów od folderu.
     */
    private const float HeavyRegionDistance = 900f;

    /*
     * Minimalna/maksymalna liczba kolumn w regionie podów.
     */
    private const int HeavyMinColumns = 4;
    private const int HeavyMaxColumns = 18;

    /*
     * Mały jitter, żeby pody nie wyglądały zbyt sztucznie.
     */
    private const float HeavyPodJitter = 20f;

    /*
     * Pod activity / importance.
     */
    private const float GlowActivityWeight = 2.0f;
    private const float SizeImportanceWeight = 0.35f;

    /*
     * Hover / picking podów.
     */
    private const float PodPickPadding = 48f;

    private readonly Dictionary<string, List<SceneNode>> _children = new();
    private readonly Dictionary<string, int> _weights = new();
    private readonly HashSet<string> _visited = new();

    private readonly List<LayoutPodInfo> _visiblePods = [];

    private float _lastFitScale = 1.0f;
    private float _energy;

    public float Energy => _energy;

    /*
     * Te metadane są przeznaczone dla warstwy interakcji / HUD / label planner.
     *
     * Domyślnie NIE renderuj labeli podów stale.
     * Renderuj je dopiero po hoverze / selection.
     */
    public IReadOnlyList<LayoutPodInfo> VisiblePods => _visiblePods;

    internal sealed class LayoutPodInfo
    {
        public string Id { get; init; } = "";
        public string ParentId { get; init; } = "";
        public string Label { get; init; } = "";

        public Vec2 Center { get; set; }
        public float Radius { get; set; }

        public float ActivityScore { get; init; }
        public float ImportanceScore { get; init; }

        public IReadOnlyList<string> FileIds { get; init; } = [];

        public Vec2 LabelPosition =>
            Center + new Vec2(
                0f,
                -Radius - 36f);

        public bool Contains(
            Vec2 worldPosition,
            float padding = PodPickPadding)
        {
            var delta =
                worldPosition - Center;

            float pickRadius =
                Radius + padding;

            return
                delta.LengthSq <= pickRadius * pickRadius;
        }
    }

    private sealed class LayoutPod
    {
        public string Id { get; init; } = "";
        public string ParentId { get; init; } = "";
        public string Label { get; init; } = "";

        public List<SceneNode> Files { get; init; } = [];

        public Vec2 Center { get; set; }
        public float Radius { get; set; }

        public float ActivityScore { get; set; }
        public float ImportanceScore { get; set; }
    }

    public void Step(
        IReadOnlyDictionary<string, SceneNode> nodes,
        float deltaSeconds)
    {
        _visiblePods.Clear();

        if (nodes.Count == 0)
        {
            _energy = 0f;
            return;
        }

        BuildTree(nodes);

        if (!nodes.TryGetValue(RootId, out var root))
        {
            root = nodes.Values.FirstOrDefault(x => x.Kind == NodeKind.Root);
        }

        if (root == null)
        {
            _energy = 0f;
            return;
        }

        BuildWeights(root.Id);

        PlaceRoot(root);

        var visualRoot =
            GetVisualRoot(root);

        LayoutRoot(
            root,
            visualRoot);

        FitLayoutToRadius(nodes);

        AnimateToHome(nodes);
    }

    public LayoutPodInfo? HitTestPod(
        Vec2 worldPosition,
        float padding = PodPickPadding)
    {
        LayoutPodInfo? best = null;
        float bestDistanceSq = float.MaxValue;

        foreach (var pod in _visiblePods)
        {
            var delta =
                worldPosition - pod.Center;

            float distanceSq =
                delta.LengthSq;

            float pickRadius =
                pod.Radius + padding;

            if (distanceSq > pickRadius * pickRadius)
                continue;

            if (distanceSq >= bestDistanceSq)
                continue;

            best = pod;
            bestDistanceSq = distanceSq;
        }

        return best;
    }

    private void AnimateToHome(
        IReadOnlyDictionary<string, SceneNode> nodes)
    {
        float energy = 0f;

        foreach (var node in nodes.Values)
        {
            if (node.Pinned)
                continue;

            var delta =
                node.HomePosition - node.Position;

            energy +=
                delta.LengthSq;

            if (delta.LengthSq < 0.25f)
            {
                node.Position = node.HomePosition;
                node.Velocity = Vec2.Zero;
                node.Force = Vec2.Zero;
                continue;
            }

            node.Position =
                Vec2.Lerp(
                    node.Position,
                    node.HomePosition,
                    AnimSmooth);

            node.Velocity = Vec2.Zero;
            node.Force = Vec2.Zero;
        }

        _energy = energy;
    }

    private SceneNode GetVisualRoot(
        SceneNode root)
    {
        if (!_children.TryGetValue(root.Id, out var rootChildren))
            return root;

        var dirs =
            rootChildren
                .Where(x => x.Kind != NodeKind.File)
                .ToList();

        var files =
            rootChildren
                .Where(x => x.Kind == NodeKind.File)
                .ToList();

        if (dirs.Count == 1 && files.Count == 0)
        {
            var container =
                dirs[0];

            container.HomePosition = root.HomePosition;
            container.Position = root.HomePosition;
            container.Velocity = Vec2.Zero;
            container.Force = Vec2.Zero;

            return container;
        }

        return root;
    }

    private void BuildTree(
        IReadOnlyDictionary<string, SceneNode> nodes)
    {
        _children.Clear();

        foreach (var node in nodes.Values)
        {
            node.Force = Vec2.Zero;

            if (node.Kind == NodeKind.Root ||
                node.Id == RootId)
            {
                continue;
            }

            var parentId =
                string.IsNullOrWhiteSpace(node.ParentId)
                    ? RootId
                    : node.ParentId;

            node.ParentId = parentId;

            if (!_children.TryGetValue(parentId, out var list))
            {
                list = new List<SceneNode>();
                _children[parentId] = list;
            }

            list.Add(node);
        }
    }

    private void BuildWeights(
        string rootId)
    {
        _weights.Clear();
        _visited.Clear();

        /*
         * Liczymy kilka wejść, bo repo root może mieć różne ID:
         * - "__repo_root__"
         * - realny root z NodeKind.Root
         * - kontener katalogu pod rootem
         */
        Weight(RootId);
        Weight(rootId);

        foreach (var parentId in _children.Keys)
            Weight(parentId);
    }

    private int Weight(
        string id)
    {
        if (!_visited.Add(id))
            return _weights.GetValueOrDefault(id, 1);

        int w = 1;

        if (_children.TryGetValue(id, out var children))
        {
            foreach (var child in children)
                w += Weight(child.Id);
        }

        _weights[id] = w;

        return w;
    }

    private void PlaceRoot(
        SceneNode root)
    {
        root.Pinned = true;
        root.Position = Vec2.Zero;
        root.HomePosition = Vec2.Zero;
        root.Velocity = Vec2.Zero;
        root.Force = Vec2.Zero;
    }

    private void LayoutRoot(
        SceneNode realRoot,
        SceneNode visualRoot)
    {
        if (!_children.TryGetValue(visualRoot.Id, out var children))
            return;

        var dirs =
            GetDirs(children);

        var files =
            GetFiles(children);

        if (files.Count > 0)
        {
            LayoutFiles(
                realRoot,
                files);
        }

        LayoutChildrenInSector(
            realRoot,
            dirs,
            centerAngle: 0f,
            sector: MathF.Tau,
            depth: 1,
            minRadius: MinRootRadius);
    }

    private void LayoutDirectory(
        SceneNode parent,
        float centerAngle,
        float sector,
        int depth)
    {
        if (!_children.TryGetValue(parent.Id, out var children))
            return;

        var dirs =
            GetDirs(children);

        var files =
            GetFiles(children);

        if (files.Count > 0)
        {
            LayoutFiles(
                parent,
                files);
        }

        if (dirs.Count == 0)
            return;

        LayoutChildrenInSector(
            parent,
            dirs,
            centerAngle,
            sector,
            depth,
            MinChildRadius + depth * 120f);
    }

    private void LayoutChildrenInSector(
        SceneNode parent,
        IReadOnlyList<SceneNode> dirs,
        float centerAngle,
        float sector,
        int depth,
        float minRadius)
    {
        if (dirs.Count == 0)
            return;

        if (dirs.Count == 1)
        {
            LayoutSingleDirectory(
                parent,
                dirs[0],
                centerAngle,
                sector,
                depth,
                minRadius);

            return;
        }

        float totalNeeded = 0f;

        var radii =
            new float[dirs.Count];

        for (int i = 0; i < dirs.Count; i++)
        {
            float r =
                EstimateClusterRadius(
                    dirs[i]);

            radii[i] = r;
            totalNeeded += r * 2f + SiblingGap;
        }

        float safeSector =
            Clamp(
                sector,
                0.35f,
                MathF.Tau);

        float radiusByArc =
            totalNeeded / safeSector;

        float radius =
            MathF.Max(
                minRadius,
                radiusByArc);

        float totalAngle = 0f;

        var spans =
            new float[dirs.Count];

        for (int i = 0; i < dirs.Count; i++)
        {
            float span =
                (radii[i] * 2f + SiblingGap) / radius;

            spans[i] = span;
            totalAngle += span;
        }

        if (totalAngle > safeSector)
        {
            radius *= totalAngle / safeSector;

            totalAngle = 0f;

            for (int i = 0; i < dirs.Count; i++)
            {
                spans[i] =
                    (radii[i] * 2f + SiblingGap) / radius;

                totalAngle += spans[i];
            }
        }

        float cursor =
            centerAngle - totalAngle * 0.5f;

        for (int i = 0; i < dirs.Count; i++)
        {
            var dir =
                dirs[i];

            float span =
                spans[i];

            float angle =
                cursor + span * 0.5f;

            float radialOffset =
                MathF.Min(
                    280f,
                    MathF.Sqrt(
                        _weights.GetValueOrDefault(
                            dir.Id,
                            1)) * 8f);

            float layerOffset =
                GetLayerOffset(i);

            float finalRadius =
                MathF.Max(
                    minRadius,
                    radius + radialOffset + layerOffset);

            var target =
                parent.HomePosition +
                Dir(angle) * finalRadius;

            SetTarget(
                dir,
                target);

            float childSector =
                Clamp(
                    span * 1.35f,
                    0.30f,
                    MathF.PI * 0.78f);

            LayoutDirectory(
                dir,
                angle,
                childSector,
                depth + 1);

            cursor += span;
        }
    }

    private void LayoutSingleDirectory(
        SceneNode parent,
        SceneNode only,
        float centerAngle,
        float sector,
        int depth,
        float minRadius)
    {
        float clusterRadius =
            minRadius +
            EstimateClusterRadius(only) * 1.10f;

        var target =
            parent.HomePosition +
            Dir(centerAngle) * clusterRadius;

        SetTarget(
            only,
            target);

        LayoutDirectory(
            only,
            centerAngle,
            Clamp(
                sector * 0.68f,
                0.35f,
                MathF.PI * 0.95f),
            depth + 1);
    }

    private static float GetLayerOffset(
        int index)
    {
        return (index % 4) switch
        {
            0 => -110f,
            1 => 20f,
            2 => 150f,
            _ => 250f
        };
    }

    private void LayoutFiles(
        SceneNode parent,
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return;

        if (files.Count >= HeavyFileFolderThreshold)
        {
            LayoutHeavyFilePods(
                parent,
                files);

            return;
        }

        LayoutFileRings(
            parent.HomePosition,
            parent.Id,
            files);
    }

    private void LayoutHeavyFilePods(
        SceneNode parent,
        IReadOnlyList<SceneNode> files)
    {
        var pods =
            BuildLayoutPods(
                parent,
                files);

        if (pods.Count == 0)
            return;

        int columns =
            EstimateHeavyColumns(
                pods.Count);

        int rows =
            Math.Max(
                1,
                (pods.Count + columns - 1) / columns);

        float outwardAngle =
            parent.HomePosition.LengthSq > 1f
                ? MathF.Atan2(
                    parent.HomePosition.Y,
                    parent.HomePosition.X)
                : StableAngle(parent.Id);

        float angleOffset =
            (Hash01(parent.Id, 4545) - 0.5f) *
            MathF.PI *
            0.18f;

        float regionDistance =
            HeavyRegionDistance +
            EstimateHeavyRegionRadius(pods.Count) * 0.42f;

        Vec2 regionCenter =
            parent.HomePosition +
            Dir(outwardAngle + angleOffset) *
            regionDistance;

        Vec2 axisX =
            Dir(outwardAngle + angleOffset + MathF.PI * 0.5f);

        Vec2 axisY =
            Dir(outwardAngle + angleOffset);

        var orderedPods =
            pods
                .OrderByDescending(p => p.ImportanceScore)
                .ThenByDescending(p => p.ActivityScore)
                .ThenBy(p => StableAngle(p.Id))
                .ToList();

        for (int i = 0; i < orderedPods.Count; i++)
        {
            var pod =
                orderedPods[i];

            var grid =
                IndexToCenteredGrid(
                    i,
                    columns,
                    rows,
                    orderedPods.Count);

            float jitterX =
                (Hash01(pod.Id + "::jx", 812) - 0.5f) *
                2f *
                HeavyPodJitter;

            float jitterY =
                (Hash01(pod.Id + "::jy", 913) - 0.5f) *
                2f *
                HeavyPodJitter;

            Vec2 podCenter =
                regionCenter +
                axisX * (grid.X + jitterX) +
                axisY * (grid.Y + jitterY);

            pod.Center = podCenter;
            pod.Radius =
                EstimatePodRadius(
                    pod.Files.Count);

            LayoutFilePod(
                pod);

            RegisterVisiblePod(
                pod);
        }
    }

    private List<LayoutPod> BuildLayoutPods(
        SceneNode parent,
        IReadOnlyList<SceneNode> files)
    {
        var ordered =
            files
                .OrderBy(x => StableAngle(x.Id))
                .ToList();

        var pods =
            new List<LayoutPod>();

        int podIndex = 0;

        for (int i = 0; i < ordered.Count; i += HeavyFilePodSize)
        {
            var slice =
                ordered
                    .Skip(i)
                    .Take(HeavyFilePodSize)
                    .ToList();

            float activity =
                ComputeActivityScore(slice);

            float importance =
                ComputeImportanceScore(slice);

            pods.Add(
                new LayoutPod
                {
                    Id = $"{parent.Id}::pod::{podIndex}",
                    ParentId = parent.Id,
                    Label = BuildPodLabel(
                        parent,
                        slice,
                        podIndex),
                    Files = slice,
                    ActivityScore = activity,
                    ImportanceScore = importance,
                    Radius = EstimatePodRadius(slice.Count)
                });

            podIndex++;
        }

        return pods;
    }

    private void RegisterVisiblePod(
        LayoutPod pod)
    {
        _visiblePods.Add(
            new LayoutPodInfo
            {
                Id = pod.Id,
                ParentId = pod.ParentId,
                Label = pod.Label,
                Center = pod.Center,
                Radius = pod.Radius,
                ActivityScore = pod.ActivityScore,
                ImportanceScore = pod.ImportanceScore,
                FileIds = pod.Files
                    .Select(x => x.Id)
                    .ToArray()
            });
    }

    private static string BuildPodLabel(
        SceneNode parent,
        IReadOnlyList<SceneNode> files,
        int podIndex)
    {
        var token =
            FindDominantPathToken(
                parent,
                files);

        if (!string.IsNullOrWhiteSpace(token))
            return $"{token} · {files.Count}";

        if (!string.IsNullOrWhiteSpace(parent.Label))
            return $"{parent.Label} · {podIndex + 1} · {files.Count}";

        return $"pod {podIndex + 1} · {files.Count}";
    }

    private static string? FindDominantPathToken(
        SceneNode parent,
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return null;

        var counts =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            foreach (var token in ExtractPathTokens(parent, file))
            {
                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (token.Length <= 1)
                    continue;

                if (IsWeakPodToken(token))
                    continue;

                counts[token] =
                    counts.GetValueOrDefault(token) + 1;
            }
        }

        if (counts.Count == 0)
            return null;

        var best =
            counts
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key.Length)
                .ThenBy(x => x.Key)
                .First();

        /*
         * Nie chcemy labela z tokenu, który występuje tylko raz,
         * bo wtedy label poda wygląda losowo.
         */
        if (best.Value <= 1 && files.Count > 4)
            return null;

        return best.Key;
    }

    private static IEnumerable<string> ExtractPathTokens(
        SceneNode parent,
        SceneNode file)
    {
        var raw =
            !string.IsNullOrWhiteSpace(file.Label)
                ? file.Label
                : file.Id;

        raw =
            raw.Replace('\\', '/');

        var parentLabel =
            parent.Label ?? "";

        if (!string.IsNullOrWhiteSpace(parentLabel))
        {
            parentLabel =
                parentLabel.Replace('\\', '/');

            if (raw.StartsWith(
                    parentLabel,
                    StringComparison.OrdinalIgnoreCase))
            {
                raw =
                    raw[parentLabel.Length..]
                        .TrimStart('/');
            }
        }

        var parts =
            raw.Split(
                ['/', '\\', '.', '_', '-', ' ', ':'],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        /*
         * Preferuj segmenty katalogowe / nazwowe, nie rozszerzenia.
         */
        foreach (var part in parts)
        {
            var normalized =
                NormalizePodToken(part);

            if (!string.IsNullOrWhiteSpace(normalized))
                yield return normalized;
        }
    }

    private static string NormalizePodToken(
        string token)
    {
        token =
            token.Trim();

        if (token.Length == 0)
            return "";

        if (token.Length > 28)
            token = token[..28];

        return token.ToLowerInvariant();
    }

    private static bool IsWeakPodToken(
        string token)
    {
        return token is
            "src" or
            "source" or
            "include" or
            "lib" or
            "libs" or
            "file" or
            "files" or
            "test" or
            "tests" or
            "txt" or
            "md" or
            "cc" or
            "cpp" or
            "cxx" or
            "h" or
            "hpp" or
            "cs" or
            "json" or
            "yaml" or
            "yml" or
            "xml" or
            "html" or
            "css" or
            "js" or
            "ts" or
            "tsx";
    }

    private static Vec2 IndexToCenteredGrid(
        int index,
        int columns,
        int rows,
        int total)
    {
        int row =
            index / columns;

        int col =
            index % columns;

        int podsInThisRow =
            Math.Min(
                columns,
                total - row * columns);

        float rowWidth =
            (podsInThisRow - 1) * HeavyPodSpacingX;

        float x =
            col * HeavyPodSpacingX -
            rowWidth * 0.5f;

        float y =
            row * HeavyPodSpacingY -
            (rows - 1) *
            HeavyPodSpacingY *
            0.5f;

        if ((row & 1) == 1)
            x += HeavyPodSpacingX * 0.5f;

        return new Vec2(
            x,
            y);
    }

    private void LayoutFilePod(
        LayoutPod pod)
    {
        var ordered =
            pod.Files
                .OrderBy(x => StableAngle(x.Id))
                .ToList();

        int placed = 0;
        int ring = 0;

        while (placed < ordered.Count)
        {
            float radius =
                FileRingStart +
                ring * FileRingGap;

            int capacityByCircumference =
                Math.Max(
                    MinFilesPerRing,
                    (int)((MathF.Tau * radius) / FileSpacing));

            int capacity =
                Math.Min(
                    MaxFilesPerRing,
                    capacityByCircumference);

            int remaining =
                ordered.Count - placed;

            int countOnRing =
                Math.Min(
                    capacity,
                    remaining);

            float start =
                Hash01(
                    pod.Id,
                    991 + ring) *
                MathF.Tau;

            start += ring * 0.37f;

            for (int i = 0; i < countOnRing; i++)
            {
                var file =
                    ordered[placed++];

                float angle =
                    start +
                    i / (float)countOnRing *
                    MathF.Tau;

                float jitterAngle =
                    (Hash01(file.Id, 17) - 0.5f) *
                    0.015f;

                float jitterRadius =
                    (Hash01(file.Id, 31) - 0.5f) *
                    2.0f;

                Vec2 target =
                    pod.Center +
                    Dir(angle + jitterAngle) *
                    (radius + jitterRadius);

                SetTarget(
                    file,
                    target);
            }

            ring++;
        }
    }

    private void LayoutFileRings(
        Vec2 center,
        string seed,
        IReadOnlyList<SceneNode> files)
    {
        var ordered =
            files
                .OrderBy(x => StableAngle(x.Id))
                .ToList();

        int placed = 0;
        int ring = 0;

        while (placed < ordered.Count)
        {
            float radius =
                FileRingStart +
                ring * FileRingGap;

            int capacityByCircumference =
                Math.Max(
                    MinFilesPerRing,
                    (int)((MathF.Tau * radius) / FileSpacing));

            int capacity =
                Math.Min(
                    MaxFilesPerRing,
                    capacityByCircumference);

            int remaining =
                ordered.Count - placed;

            int countOnRing =
                Math.Min(
                    capacity,
                    remaining);

            float start =
                Hash01(
                    seed,
                    991 + ring) *
                MathF.Tau;

            start += ring * 0.37f;

            for (int i = 0; i < countOnRing; i++)
            {
                var file =
                    ordered[placed++];

                float angle =
                    start +
                    i / (float)countOnRing *
                    MathF.Tau;

                float jitterAngle =
                    (Hash01(file.Id, 17) - 0.5f) *
                    0.015f;

                float jitterRadius =
                    (Hash01(file.Id, 31) - 0.5f) *
                    2.0f;

                Vec2 target =
                    center +
                    Dir(angle + jitterAngle) *
                    (radius + jitterRadius);

                SetTarget(
                    file,
                    target);
            }

            ring++;
        }
    }

    private float EstimateClusterRadius(
        SceneNode node)
    {
        int fileCount = 0;
        int dirCount = 0;

        if (_children.TryGetValue(node.Id, out var children))
        {
            foreach (var child in children)
            {
                if (child.Kind == NodeKind.File)
                    fileCount++;
                else
                    dirCount++;
            }
        }

        float fileRadius =
            fileCount >= HeavyFileFolderThreshold
                ? EstimateHeavyFileClusterRadius(fileCount)
                : EstimateFileShellRadius(fileCount);

        float dirRadius =
            dirCount > 0
                ? 160f + dirCount * 54f
                : 0f;

        float weightRadius =
            MathF.Sqrt(
                _weights.GetValueOrDefault(
                    node.Id,
                    1)) * 10f;

        return Clamp(
            MathF.Max(
                fileRadius,
                dirRadius) + weightRadius,
            160f,
            1600f);
    }

    private static float EstimateHeavyFileClusterRadius(
        int fileCount)
    {
        if (fileCount <= 0)
            return 180f;

        int podCount =
            (fileCount + HeavyFilePodSize - 1) /
            HeavyFilePodSize;

        return
            EstimateHeavyRegionRadius(podCount) +
            320f;
    }

    private static float EstimateHeavyRegionRadius(
        int podCount)
    {
        if (podCount <= 0)
            return 180f;

        int columns =
            EstimateHeavyColumns(podCount);

        int rows =
            Math.Max(
                1,
                (podCount + columns - 1) / columns);

        float width =
            Math.Max(
                1,
                columns - 1) *
            HeavyPodSpacingX;

        float height =
            Math.Max(
                1,
                rows - 1) *
            HeavyPodSpacingY;

        return
            MathF.Sqrt(width * width + height * height) * 0.5f +
            FileRingStart +
            FileRingGap * 2f;
    }

    private static float EstimatePodRadius(
        int fileCount)
    {
        if (fileCount <= 0)
            return FileRingStart;

        return EstimateFileShellRadius(fileCount);
    }

    private static int EstimateHeavyColumns(
        int podCount)
    {
        if (podCount <= 0)
            return HeavyMinColumns;

        int columns =
            (int)MathF.Ceiling(
                MathF.Sqrt(podCount * 1.25f));

        return ClampInt(
            columns,
            HeavyMinColumns,
            HeavyMaxColumns);
    }

    private static float EstimateFileShellRadius(
        int fileCount)
    {
        if (fileCount <= 0)
            return FileRingStart + 48f;

        int rings =
            EstimateFileRingCount(fileCount);

        float lastRadius =
            FileRingStart +
            MathF.Max(
                0,
                rings - 1) *
            FileRingGap;

        return lastRadius + FileRingGap + 72f;
    }

    private static int EstimateFileRingCount(
        int fileCount)
    {
        if (fileCount <= 0)
            return 0;

        int remaining = fileCount;
        int ring = 0;

        while (remaining > 0)
        {
            float radius =
                FileRingStart +
                ring * FileRingGap;

            int capacityByCircumference =
                Math.Max(
                    MinFilesPerRing,
                    (int)((MathF.Tau * radius) / FileSpacing));

            int capacity =
                Math.Min(
                    MaxFilesPerRing,
                    capacityByCircumference);

            remaining -= capacity;
            ring++;
        }

        return ring;
    }

    private static float ComputeActivityScore(
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return 0f;

        float glowSum = 0f;

        foreach (var file in files)
            glowSum += file.Glow;

        return glowSum / files.Count;
    }

    private static float ComputeImportanceScore(
        IReadOnlyList<SceneNode> files)
    {
        if (files.Count == 0)
            return 0f;

        float activity =
            ComputeActivityScore(files);

        float size =
            MathF.Sqrt(files.Count) *
            SizeImportanceWeight;

        return
            activity * GlowActivityWeight +
            size;
    }

    private void FitLayoutToRadius(
        IReadOnlyDictionary<string, SceneNode> nodes)
    {
        float maxDistance = 0f;

        foreach (var node in nodes.Values)
        {
            if (node.Kind == NodeKind.Root)
                continue;

            float distance =
                MathF.Sqrt(
                    node.HomePosition.LengthSq);

            if (distance > maxDistance)
                maxDistance = distance;
        }

        float scale = 1.0f;

        if (maxDistance > MaxLayoutRadius)
        {
            scale =
                MaxLayoutRadius / maxDistance;

            scale =
                MathF.Max(
                    MinFitScale,
                    scale);
        }

        foreach (var node in nodes.Values)
        {
            if (node.Kind == NodeKind.Root ||
                node.Pinned)
            {
                continue;
            }

            node.HomePosition =
                new Vec2(
                    node.HomePosition.X * scale,
                    node.HomePosition.Y * scale);
        }

        if (MathF.Abs(scale - 1.0f) > 0.0001f)
        {
            ScaleVisiblePods(scale);
        }

        if (MathF.Abs(scale - _lastFitScale) > FitSnapThreshold)
        {
            foreach (var node in nodes.Values)
            {
                if (node.Kind == NodeKind.Root ||
                    node.Pinned)
                {
                    continue;
                }

                node.Position = node.HomePosition;
                node.Velocity = Vec2.Zero;
                node.Force = Vec2.Zero;
            }
        }

        _lastFitScale = scale;
    }

    private void ScaleVisiblePods(
        float scale)
    {
        foreach (var pod in _visiblePods)
        {
            pod.Center =
                new Vec2(
                    pod.Center.X * scale,
                    pod.Center.Y * scale);

            pod.Radius *= scale;
        }
    }

    private static List<SceneNode> GetDirs(
        IReadOnlyList<SceneNode> children)
    {
        return children
            .Where(x => x.Kind != NodeKind.File)
            .OrderBy(x => StableAngle(x.Id))
            .ToList();
    }

    private static List<SceneNode> GetFiles(
        IReadOnlyList<SceneNode> children)
    {
        return children
            .Where(x => x.Kind == NodeKind.File)
            .OrderBy(x => StableAngle(x.Id))
            .ToList();
    }

    private static float StableAngle(
        string id)
    {
        return Hash01(
            id,
            777) * MathF.Tau;
    }

    private static float Hash01(
        string text,
        int seed)
    {
        unchecked
        {
            int hash = seed;

            foreach (char c in text)
                hash = hash * 31 + c;

            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            hash *= unchecked((int)0x846ca68b);
            hash ^= hash >> 16;

            return
                (hash & 0x00FFFFFF) /
                (float)0x01000000;
        }
    }

    private static void SetTarget(
        SceneNode node,
        Vec2 target)
    {
        node.HomePosition = target;
        node.Velocity = Vec2.Zero;
        node.Force = Vec2.Zero;
    }

    private static Vec2 Dir(
        float angle)
    {
        return new Vec2(
            MathF.Cos(angle),
            MathF.Sin(angle));
    }

    private static float Clamp(
        float value,
        float min,
        float max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }

    private static int ClampInt(
        int value,
        int min,
        int max)
    {
        if (value < min)
            return min;

        if (value > max)
            return max;

        return value;
    }
}