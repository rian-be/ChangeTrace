using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

internal sealed class BundledEdge(string fromId, IEnumerable<string> toIds, EdgeKind kind, double createdAt)
{
    public string FromId { get; } = fromId;
    public IReadOnlyList<string> ToIds { get; } = new List<string>(toIds);
    public EdgeKind Kind { get; } = kind;
    public double CreatedAt { get; } = createdAt;
    public float Alpha { get; set; } = 1f;
}