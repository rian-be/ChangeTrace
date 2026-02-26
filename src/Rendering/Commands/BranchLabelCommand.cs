using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Commands;

/// <summary>
/// Command to spawn, update, or remove a visual label for a branch in the graph.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used by the renderer to display branch labels without exposing rendering logic to domain code.</item>
/// <item>Part of the sealed <see cref="RenderCommand"/> hierarchy for timeline-based visualization.</item>
/// <item>The <see cref="Action"/> field controls whether the label should be created, pulsed, or removed.</item>
/// </list>
/// </remarks>
internal sealed record BranchLabelCommand(
    double Timestamp,
    string BranchName,
    BranchLabelAction Action
) : RenderCommand(Timestamp);