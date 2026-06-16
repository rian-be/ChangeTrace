using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.States.Scene;

/// <summary>
/// Materializes an immutable scene snapshot from component snapshot collections.
/// </summary>
internal static class SceneSnapshotMaterializer
{
    internal static SceneSnapshot Create(
        IReadOnlyList<NodeSnapshot> nodes,
        IReadOnlyList<AvatarSnapshot> avatars,
        IReadOnlyList<EdgeSnapshotIndexed> edges,
        IReadOnlyList<ParticleSnapshot> particles)
    {
        var orderedNodes = CreateOrderedNodes(nodes);
        var nodePositionIndex = BuildNodePositionIndex(orderedNodes);
        var originalToSortedIndex = BuildOriginalToSortedIndex(nodes, nodePositionIndex);
        var remappedEdges = CreateEdges(edges, originalToSortedIndex);

        return new SceneSnapshot(
            orderedNodes,
            avatars,
            remappedEdges,
            particles,
            nodePositionIndex);
    }

    internal static IReadOnlyList<NodeSnapshot> CreateOrderedNodes(
        IReadOnlyList<NodeSnapshot> nodes)
    {
        var order = new int[nodes.Count];

        for (var i = 0; i < nodes.Count; i++)
            order[i] = i;

        Array.Sort(
            order,
            (left, right) =>
            {
                var kindOrder =
                    GetNodeOrder(nodes[left].Kind)
                        .CompareTo(GetNodeOrder(nodes[right].Kind));

                if (kindOrder != 0)
                    return kindOrder;

                return string.CompareOrdinal(nodes[left].Id, nodes[right].Id);
            });

        return order.Length == 0
            ? []
            : new OrderedNodeSnapshotList(nodes, order);
    }

    internal static Dictionary<string, int> BuildNodePositionIndex(
        IReadOnlyList<NodeSnapshot> nodes)
    {
        var index = new Dictionary<string, int>(nodes.Count);

        for (var i = 0; i < nodes.Count; i++)
            index[nodes[i].Id] = i;

        return index;
    }

    internal static int[] BuildOriginalToSortedIndex(
        IReadOnlyList<NodeSnapshot> nodes,
        IReadOnlyDictionary<string, int> nodePositionIndex)
    {
        var remap = new int[nodes.Count];

        for (var i = 0; i < nodes.Count; i++)
            remap[i] = nodePositionIndex[nodes[i].Id];

        return remap;
    }

    internal static IReadOnlyList<EdgeSnapshotIndexed> CreateEdges(
        IReadOnlyList<EdgeSnapshotIndexed> edges,
        int[] originalToSortedIndex)
    {
        var edgeIndexes = new List<int>(edges.Count);
        var seenEdges = new HashSet<ulong>(edges.Count);

        for (var i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];

            if ((uint)edge.FromIndex >= (uint)originalToSortedIndex.Length ||
                (uint)edge.ToIndex >= (uint)originalToSortedIndex.Length)
            {
                continue;
            }

            var remappedFromIndex = originalToSortedIndex[edge.FromIndex];
            var remappedToIndex = originalToSortedIndex[edge.ToIndex];
            var edgeKey = PackEdgeKey(remappedFromIndex, remappedToIndex, edge.Kind);

            if (!seenEdges.Add(edgeKey))
                continue;

            edgeIndexes.Add(i);
        }

        return edgeIndexes.Count == 0
            ? []
            : new RemappedEdgeSnapshotList(edges, edgeIndexes.ToArray(), originalToSortedIndex);
    }

    private static int GetNodeOrder(NodeKind kind) =>
        kind switch
        {
            NodeKind.Root => 0,
            NodeKind.Branch => 1,
            NodeKind.File => 2,
            _ => 1
        };

    private static ulong PackEdgeKey(
        int fromIndex,
        int toIndex,
        EdgeKind kind) =>
        ((ulong)(uint)fromIndex << 32) |
        ((ulong)(uint)toIndex << 8) |
        (byte)kind;
}
