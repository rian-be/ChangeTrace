using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Commands;

internal sealed record BundledEdgeCommand(
    string FromNode,
    IReadOnlyList<string> ToNodes,
    EdgeKind Kind,
    double Timestamp
) : RenderCommand(Timestamp);

