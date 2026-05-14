using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene.Relations;

/// <summary>
/// Represents bundled directed edge from a single source node to multiple target nodes.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the source node via <see cref="SceneRelation.FromId"/> and multiple target nodes via <see cref="Targets"/>.</item>
/// <item>Maintains <see cref="SceneRelation.Kind"/> to indicate the type of relation (e.g., commit, PR).</item>
/// <item>Supports <see cref="SceneRelation.Alpha"/> for fade-out animation of the entire bundle.</item>
/// <item>Records <see cref="SceneRelation.CreatedAt"/> virtual time for temporal ordering and visual effects.</item>
/// </list>
/// </remarks>
internal sealed class BundledEdge(string fromId, IEnumerable<string> targets, EdgeKind kind, double createdAt, System.Numerics.Vector4? color = null)
    : SceneRelation(fromId, kind, createdAt, color)
{
    /// <summary>
    /// Gets readonly lists of target node IDs in this bundle.
    /// </summary>
    public IReadOnlyList<string> Targets { get; } = targets.ToArray();
}