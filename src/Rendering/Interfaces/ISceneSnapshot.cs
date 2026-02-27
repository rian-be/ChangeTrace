using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines read only snapshot of scene at specific moment in time.
/// </summary>
/// <remarks>
/// Snapshot provides immutable collections of <see cref="NodeSnapshot"/>, 
/// <see cref="AvatarSnapshot"/>, <see cref="EdgeSnapshot"/>, and <see cref="ParticleSnapshot"/> objects.
/// <para>
/// Includes helper methods for filtering, spatial queries, and computing basic scene statistics.
/// </para>
/// </remarks>
internal interface ISceneSnapshot
{
    /// <summary>
    /// All nodes in snapshot.
    /// </summary>
    IReadOnlyList<NodeSnapshot> Nodes { get; }

    /// <summary>
    /// All avatars in snapshot.
    /// </summary>
    IReadOnlyList<AvatarSnapshot> Avatars { get; }

    /// <summary>
    /// All edges in snapshot.
    /// </summary>
    IReadOnlyList<EdgeSnapshot> Edges { get; }

    /// <summary>
    /// All particles in snapshot.
    /// </summary>
    IReadOnlyList<ParticleSnapshot> Particles { get; }

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    /// <param name="id">Node identifier.</param>
    /// <returns>Matching <see cref="NodeSnapshot"/> or <c>null</c> if not found.</returns>
    NodeSnapshot? FindNode(string id);

    /// <summary>
    /// Returns nodes of pecific <see cref="NodeKind"/>.
    /// </summary>
    /// <param name="kind">Node type filter.</param>
    /// <returns>Enumerable of matching nodes.</returns>
    IEnumerable<NodeSnapshot> NodesOfKind(NodeKind kind);

    /// <summary>
    /// Returns nodes whose glow exceeds specified threshold.
    /// </summary>
    /// <param name="threshold">Glow threshold.</param>
    /// <returns>Enumerable of glowing nodes.</returns>
    IEnumerable<NodeSnapshot> GlowingNodes(float threshold = 0.05f);

    /// <summary>
    /// Returns avatars with activity above given threshold.
    /// </summary>
    /// <param name="activityThreshold">Minimum activity level.</param>
    /// <returns>Enumerable of active avatars.</returns>
    IEnumerable<AvatarSnapshot> ActiveAvatars(float activityThreshold = 0.1f);

    /// <summary>
    /// Returns avatars visible above specified alpha threshold.
    /// </summary>
    /// <param name="alphaThreshold">Minimum alpha.</param>
    /// <returns>Enumerable of visible avatars.</returns>
    IEnumerable<AvatarSnapshot> VisibleAvatars(float alphaThreshold = 0.05f);

    /// <summary>
    /// Finds avatar by actor identifier.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <returns>Matching <see cref="AvatarSnapshot"/> or <c>null</c> if not found.</returns>
    AvatarSnapshot? FindAvatar(string actor);

    /// <summary>
    /// Returns edges originating from specified node.
    /// </summary>
    /// <param name="nodeId">Source node identifier.</param>
    /// <returns>Enumerable of edges.</returns>
    IEnumerable<EdgeSnapshot> EdgesFrom(string nodeId);

    /// <summary>
    /// Returns edges terminating at specified node.
    /// </summary>
    /// <param name="nodeId">Target node identifier.</param>
    /// <returns>Enumerable of edges.</returns>
    IEnumerable<EdgeSnapshot> EdgesTo(string nodeId);

    /// <summary>
    /// Returns edges of given <see cref="EdgeKind"/>.
    /// </summary>
    /// <param name="kind">Edge type filter.</param>
    /// <returns>Enumerable of edges.</returns>
    IEnumerable<EdgeSnapshot> EdgesOfKind(EdgeKind kind);

    /// <summary>
    /// Returns edges visible above specified alpha threshold.
    /// </summary>
    /// <param name="alphaThreshold">Minimum alpha.</param>
    /// <returns>Enumerable of visible edges.</returns>
    IEnumerable<EdgeSnapshot> VisibleEdges(float alphaThreshold = 0.02f);

    /// <summary>
    /// Computes axis aligned bounding box containing all nodes, or null if empty.
    /// </summary>
    /// <returns>Bounding <see cref="Bounds"/> or <c>null</c>.</returns>
    Bounds? NodeBounds();

    /// <summary>
    /// Computes geometric center of all nodes, or null if none exist.
    /// </summary>
    /// <returns>Center position or <c>null</c>.</returns>
    Vec2? NodesCenter();

    /// <summary>
    /// Finds node closest to given point.
    /// </summary>
    /// <param name="point">Point in scene space.</param>
    /// <returns>Closest <see cref="NodeSnapshot"/> or <c>null</c>.</returns>
    NodeSnapshot? ClosestNode(Vec2 point);

    /// <summary>
    /// Computes basic statistics of scene snapshot.
    /// </summary>
    /// <returns><see cref="SceneStats"/> object.</returns>
    SceneStats ComputeStats();

    /// <summary>Total number of nodes.</summary>
    int NodeCount { get; }

    /// <summary>Total number of avatars.</summary>
    int AvatarCount { get; }

    /// <summary>Total number of edges.</summary>
    int EdgeCount { get; }

    /// <summary>Total number of particles.</summary>
    int ParticleCount { get; }

    /// <summary>Total number of all objects in snapshot.</summary>
    int TotalObjects { get; }

    /// <summary>True if snapshot contains no objects.</summary>
    bool IsEmpty { get; }

    /// <summary>True if snapshot contains any particles.</summary>
    bool HasParticles { get; }
}