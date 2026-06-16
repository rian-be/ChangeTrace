using System.Collections;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Lightweight remapped edge snapshot view over an existing edge buffer.
/// </summary>
internal sealed class RemappedEdgeSnapshotList(
    IReadOnlyList<EdgeSnapshotIndexed> source,
    int[] edgeIndexes,
    int[] remap)
    : IReadOnlyList<EdgeSnapshotIndexed>
{
    public int Count => edgeIndexes.Length;

    public EdgeSnapshotIndexed this[int index]
    {
        get
        {
            var edge = source[edgeIndexes[index]];
            return edge with
            {
                FromIndex = remap[edge.FromIndex],
                ToIndex = remap[edge.ToIndex]
            };
        }
    }

    public IEnumerator<EdgeSnapshotIndexed> GetEnumerator()
    {
        for (var i = 0; i < edgeIndexes.Length; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
