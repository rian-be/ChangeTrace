using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene.Relations;

/// <summary>
/// Represents directed edge between two nodes in the scene.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the source node via <see cref="SceneRelation.FromId"/> and the target node via <see cref="ToId"/>.</item>
/// <item>Maintains <see cref="SceneRelation.Kind"/> to indicate the type of relation (e.g., commit, PR).</item>
/// <item>Supports <see cref="SceneRelation.Alpha"/> for fade-out animation over time.</item>
/// <item>Records <see cref="SceneRelation.CreatedAt"/> virtual time for temporal ordering and visual effects.</item>
/// </list>
/// </remarks>
internal sealed class SceneEdge(string fromId, string toId, EdgeKind kind, double createdAt, System.Numerics.Vector4? color = null)
    : SceneRelation(fromId, kind, createdAt, color)
{
    /// <summary>
    /// Gets the target node ID of this edge.
    /// </summary>
    public string ToId { get; } = toId;
}