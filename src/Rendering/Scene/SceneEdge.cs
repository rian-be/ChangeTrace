using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Represents directed edge between two nodes in scene.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks source (<see cref="FromId"/>) and target (<see cref="ToId"/>) node IDs.</item>
/// <item>Maintains <see cref="Kind"/> to indicate edge type (commit, PR, etc.).</item>
/// <item>Supports <see cref="Alpha"/> for fade out animation over time.</item>
/// <item>Records <see cref="CreatedAt"/> virtual time for temporal ordering and effects.</item>
/// </list>
/// </remarks>
internal sealed class SceneEdge
{
    /// <summary>
    /// ID of source node.
    /// </summary>
    internal string FromId { get; }

    /// <summary>
    /// ID of target node.
    /// </summary>
    internal string ToId { get; }

    /// <summary>
    /// Type of edge (commit, pull request, etc.).
    /// </summary>
    internal EdgeKind Kind { get; }

    /// <summary>
    /// Transparency of edge; fades out over time.
    /// </summary>
    internal float Alpha { get; set; } = 1f;

    /// <summary>
    /// Virtual time at which edge was created.
    /// </summary>
    internal double CreatedAt { get; }

    /// <summary>
    /// Initializes new <see cref="SceneEdge"/>.
    /// </summary>
    /// <param name="fromId">Source node ID.</param>
    /// <param name="toId">Target node ID.</param>
    /// <param name="kind">Type of edge.</param>
    /// <param name="createdAt">Virtual time when edge was created.</param>
    internal SceneEdge(string fromId, string toId, EdgeKind kind, double createdAt)
    {
        FromId = fromId;
        ToId = toId;
        Kind = kind;
        CreatedAt = createdAt;
    }
}