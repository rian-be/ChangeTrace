using System.Collections;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Lightweight ordered node snapshot view over an existing node buffer.
/// </summary>
internal sealed class OrderedNodeSnapshotList(
    IReadOnlyList<NodeSnapshot> source,
    int[] order)
    : IReadOnlyList<NodeSnapshot>
{
    public int Count => order.Length;

    public NodeSnapshot this[int index] =>
        source[order[index]];

    public IEnumerator<NodeSnapshot> GetEnumerator()
    {
        foreach (var item in order)
            yield return source[item];
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}
