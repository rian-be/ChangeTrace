using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Commands;

/// <summary>
/// Command to draw an edge between two nodes in the graph.
/// Renderer interprets <see cref="Kind"/> and <see cref="Intensity"/> to determine visual effect.
/// </summary>
/// <param name="Timestamp">Virtual time (seconds) when this command occurs.</param>
/// <param name="FromNode">Source node of the edge.</param>
/// <param name="ToNode">Target node of the edge.</param>
/// <param name="Kind">Type of edge (commit, merge, pull request).</param>
/// <param name="Intensity">Fade intensity of the edge (0.0–1.0).</param>
internal sealed record EdgeCommand(
    double Timestamp,
    string FromNode,
    string ToNode,
    EdgeKind Kind,
    float Intensity = 1f
) : RenderCommand(Timestamp);