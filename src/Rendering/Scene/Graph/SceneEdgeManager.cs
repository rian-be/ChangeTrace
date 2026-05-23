using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene.Relations;

namespace ChangeTrace.Rendering.Scene.Graph;

/// <summary>
/// Manages runtime and hierarchy scene edges.
/// </summary>
internal sealed class SceneEdgeManager
{
    /// <summary>
    /// Runtime scene edges.
    /// </summary>
    private readonly List<SceneEdge> _runtimeEdges =
        [];

    /// <summary>
    /// Temporary scene relations.
    /// </summary>
    private readonly List<SceneRelation> _relations =
        [];

    /// <summary>
    /// Bundled runtime edges.
    /// </summary>
    private readonly List<BundledEdge> _bundledEdges =
        [];

    /// <summary>
    /// Temporary dedupe cache for bundled targets.
    /// </summary>
    private readonly HashSet<string> _dedupeTargets =
        [];

    /// <summary>
    /// Cached combined edge list.
    /// </summary>
    private List<SceneEdge> _cachedEdges =
        [];

    /// <summary>
    /// Indicates runtime cache invalidation.
    /// </summary>
    private bool _runtimeDirty =
        true;

    /// <summary>
    /// Last processed hierarchy version.
    /// </summary>
    private int _lastHierarchyVersion =
        -1;

    /// <summary>
    /// Returns combined runtime and hierarchy edges.
    /// </summary>
    public IReadOnlyList<SceneEdge> GetEdges(
        IReadOnlyDictionary<string, SceneNode> nodes,
        SceneHierarchyManager hierarchy)
    {
        IReadOnlyList<SceneEdge> hierarchyEdges =
            hierarchy.Edges;

        if (!_runtimeDirty &&
            _lastHierarchyVersion == hierarchy.Version)
        {
            return _cachedEdges;
        }

        RebuildCache(
            nodes,
            hierarchyEdges);

        _runtimeDirty = false;
        _lastHierarchyVersion = hierarchy.Version;

        return _cachedEdges;
    }

    /// <summary>
    /// Adds runtime edge between two nodes.
    /// </summary>
    public void AddEdge(
        IReadOnlyDictionary<string, SceneNode> nodes,
        string fromId,
        string toId,
        EdgeKind kind,
        double virtualTime)
    {
        fromId =
            SceneNodeIds.Normalize(
                fromId,
                NodeKind.File);

        toId =
            SceneNodeIds.Normalize(
                toId,
                NodeKind.File);

        if (string.IsNullOrWhiteSpace(fromId) ||
            string.IsNullOrWhiteSpace(toId) ||
            !nodes.ContainsKey(fromId) ||
            !nodes.ContainsKey(toId))
        {
            return;
        }

        _runtimeEdges.Add(
            new SceneEdge(
                fromId,
                toId,
                kind,
                virtualTime)
            {
                Life = 0.6f
            });

        _runtimeDirty = true;
    }

    /// <summary>
    /// Adds bundled edge between source and multiple targets.
    /// </summary>
    public void AddBundledEdge(
        IReadOnlyDictionary<string, SceneNode> nodes,
        string fromId,
        IEnumerable<string> toIds,
        EdgeKind kind,
        double virtualTime)
    {
        fromId =
            SceneNodeIds.Normalize(
                fromId,
                NodeKind.File);

        if (string.IsNullOrWhiteSpace(fromId))
            return;

        List<string> targets =
            BuildTargetList(
                nodes,
                toIds);

        if (targets.Count == 0)
            return;

        _bundledEdges.Add(
            new BundledEdge(
                fromId,
                targets,
                kind,
                virtualTime)
            {
                Life = 0.6f
            });

        _runtimeDirty = true;
    }

    /// <summary>
    /// Updates edge lifetime and alpha decay.
    /// </summary>
    public void Tick(
        double dt,
        float decayRate)
    {
        float delta =
            (float)dt * decayRate;

        bool changed =
            false;

        changed |= TickEdges(
            _runtimeEdges,
            delta);

        changed |= TickRelations(
            _relations,
            delta);

        changed |= TickBundledEdges(
            _bundledEdges,
            delta);

        if (changed)
            _runtimeDirty = true;
    }

    /// <summary>
    /// </summary>
    public void MarkDirty()
    {
        _runtimeDirty = true;
    }

    /// <summary>
    /// Marks topology and hierarchy cache as dirty.
    /// </summary>
    public void MarkTopologyDirty()
    {
        _runtimeDirty = true;
        _lastHierarchyVersion = -1;
    }

    /// <summary>
    /// Clears all runtime edge state.
    /// </summary>
    public void Clear()
    {
        _runtimeEdges.Clear();
        _relations.Clear();

        _bundledEdges.Clear();
        _cachedEdges.Clear();

        _dedupeTargets.Clear();

        _runtimeDirty = true;
        _lastHierarchyVersion = -1;
    }

    /// <summary>
    /// Rebuilds the combined runtime edge cache.
    /// </summary>
    private void RebuildCache(
        IReadOnlyDictionary<string, SceneNode> nodes,
        IReadOnlyList<SceneEdge> hierarchyEdges)
    {
        int totalCount =
            hierarchyEdges.Count +
            _runtimeEdges.Count;

        foreach (BundledEdge t in _bundledEdges)
            totalCount += t.Targets.Count;

        EnsureCacheCapacity(totalCount);

        _cachedEdges.Clear();

        AddRange(
            _cachedEdges,
            hierarchyEdges);

        AddRange(
            _cachedEdges,
            _runtimeEdges);

        foreach (BundledEdge t in _bundledEdges)
        {
            AddBundledEdgesToCache(
                nodes,
                t);
        }
    }

    /// <summary>
    /// Ensures cached edge capacity.
    /// </summary>
    private void EnsureCacheCapacity(int requiredCapacity)
    {
        if (_cachedEdges.Capacity >= requiredCapacity)
            return;

        _cachedEdges =
            new List<SceneEdge>(
                requiredCapacity + 128);
    }

    /// <summary>
    /// Expands bundled edges into the runtime cache.
    /// </summary>
    private void AddBundledEdgesToCache(
        IReadOnlyDictionary<string, SceneNode> nodes,
        BundledEdge bundled)
    {
        IReadOnlyList<string> targets =
            bundled.Targets;

        foreach (string targetId in targets)
        {
            if (!nodes.ContainsKey(targetId))
                continue;

            _cachedEdges.Add(
                new SceneEdge(
                    bundled.FromId,
                    targetId,
                    bundled.Kind,
                    bundled.CreatedAt,
                    bundled.Color)
                {
                    Alpha = bundled.Alpha,
                    Life = bundled.Life
                });
        }
    }

    /// <summary>
    /// Builds deduplicated bundled edge target list.
    /// </summary>
    private List<string> BuildTargetList(
        IReadOnlyDictionary<string, SceneNode> nodes,
        IEnumerable<string> toIds)
    {
        List<string> targets =
            [];

        _dedupeTargets.Clear();

        foreach (string rawId in toIds)
        {
            string id =
                SceneNodeIds.Normalize(
                    rawId,
                    NodeKind.File);

            if (string.IsNullOrWhiteSpace(id))
                continue;

            if (!nodes.ContainsKey(id))
                continue;

            if (!_dedupeTargets.Add(id))
                continue;

            targets.Add(id);
        }

        _dedupeTargets.Clear();

        return targets;
    }

    /// <summary>
    /// Updates runtime edge lifetime state.
    /// </summary>
    private static bool TickEdges(
        List<SceneEdge> edges,
        float delta)
    {
        bool changed =
            false;

        for (int i = edges.Count - 1; i >= 0; i--)
        {
            SceneEdge edge =
                edges[i];

            edge.Life -= delta;

            edge.Alpha =
                Math.Clamp(
                    edge.Life / 0.5f,
                    0f,
                    1f);

            if (edge.Life > 0f)
                continue;

            RemoveAtSwapBack(
                edges,
                i);

            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Updates relation lifetime state.
    /// </summary>
    private static bool TickRelations(
        List<SceneRelation> relations,
        float delta)
    {
        bool changed =
            false;

        for (int i = relations.Count - 1; i >= 0; i--)
        {
            SceneRelation relation =
                relations[i];

            relation.Life -= delta;

            relation.Alpha =
                Math.Clamp(
                    relation.Life / 0.5f,
                    0f,
                    1f);

            if (relation.Life > 0f)
                continue;

            RemoveAtSwapBack(
                relations,
                i);

            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Updates bundled edge lifetime state.
    /// </summary>
    private static bool TickBundledEdges(
        List<BundledEdge> edges,
        float delta)
    {
        bool changed =
            false;

        for (int i = edges.Count - 1; i >= 0; i--)
        {
            BundledEdge edge =
                edges[i];

            edge.Life -= delta;

            edge.Alpha =
                Math.Clamp(
                    edge.Life / 0.5f,
                    0f,
                    1f);

            if (edge.Life > 0f)
                continue;

            RemoveAtSwapBack(
                edges,
                i);

            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Appends all items from the source into a target.
    /// </summary>
    private static void AddRange<T>(
        List<T> target,
        IReadOnlyList<T> source)
    {
        foreach (T t in source)
            target.Add(t);
    }

    /// <summary>
    /// Removes list item using swap-back removal.
    /// </summary>
    private static void RemoveAtSwapBack<T>(
        List<T> list,
        int index)
    {
        int last =
            list.Count - 1;

        if (index != last)
            list[index] = list[last];

        list.RemoveAt(last);
    }
}