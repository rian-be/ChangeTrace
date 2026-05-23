using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene;

/// <summary>
/// Base class representing a directed relation originating from a single node in the scene.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Tracks the source node via <see cref="FromId"/>.</item>
/// <item>Specifies the type of relation using <see cref="Kind"/>.</item>
/// <item>Records <see cref="CreatedAt"/> virtual time for temporal ordering and visual effects.</item>
/// <item>Supports <see cref="Alpha"/> for fade-out animations.</item>
/// </list>
/// </remarks>
internal abstract class SceneRelation(string fromId, EdgeKind kind, double createdAt, System.Numerics.Vector4? color = null)
{
    /// <summary>
    /// Gets or sets the color of the relation.
    /// </summary>
    public System.Numerics.Vector4 Color { get; set; } = color ?? new System.Numerics.Vector4(1, 1, 1, 1);

    /// <summary>
    /// Gets the source node ID of the relation.
    /// </summary>
    public string FromId { get; } = fromId;

    /// <summary>
    /// Gets the type of the relation (e.g., commit, PR).
    /// </summary>
    public EdgeKind Kind { get; } = kind;

    /// <summary>
    /// Gets the virtual creation time of the relation for temporal ordering and effects.
    /// </summary>
    public double CreatedAt { get; } = createdAt;

    /// <summary>
    /// Gets or sets the alpha transparency of the relation for fade-in/fade-out animations.
    /// </summary>
    public float Alpha { get; set; } = 1f;

    /// <summary>
    /// Gets or sets the remaining life of the relation (in real seconds).
    /// </summary>
    public float Life { get; set; } = 1.0f;
}