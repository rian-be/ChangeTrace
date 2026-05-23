using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Immutable snapshot of scene state at a specific moment in time.
/// </summary>
internal interface ISceneSnapshot
{
    /// <summary>
    /// Snapshot nodes.
    /// </summary>
    IReadOnlyList<NodeSnapshot> Nodes { get; }

    /// <summary>
    /// Snapshot avatars.
    /// </summary>
    IReadOnlyList<AvatarSnapshot> Avatars { get; }

    /// <summary>
    /// Snapshot edges.
    /// </summary>
    IReadOnlyList<EdgeSnapshot> Edges { get; }

    /// <summary>
    /// Snapshot particles.
    /// </summary>
    IReadOnlyList<ParticleSnapshot> Particles { get; }

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    NodeSnapshot? FindNode(string id);

    /// <summary>
    /// Returns nodes matching the specified kind.
    /// </summary>
    IEnumerable<NodeSnapshot> NodesOfKind(NodeKind kind);

    /// <summary>
    /// Returns glowing nodes above threshold.
    /// </summary>
    IEnumerable<NodeSnapshot> GlowingNodes(float threshold = 0.05f);

    /// <summary>
    /// Returns active avatars above activity threshold.
    /// </summary>
    IEnumerable<AvatarSnapshot> ActiveAvatars(float activityThreshold = 0.1f);

    /// <summary>
    /// Returns visible avatars above alpha threshold.
    /// </summary>
    IEnumerable<AvatarSnapshot> VisibleAvatars(float alphaThreshold = 0.05f);

    /// <summary>
    /// Finds avatar by actor identifier.
    /// </summary>
    AvatarSnapshot? FindAvatar(string actor);

    /// <summary>
    /// Returns edges originating from the node.
    /// </summary>
    IEnumerable<EdgeSnapshot> EdgesFrom(string nodeId);

    /// <summary>
    /// Returns edges targeting the node.
    /// </summary>
    IEnumerable<EdgeSnapshot> EdgesTo(string nodeId);

    /// <summary>
    /// Returns edges matching the specified kind.
    /// </summary>
    IEnumerable<EdgeSnapshot> EdgesOfKind(EdgeKind kind);

    /// <summary>
    /// Returns visible edges above an alpha threshold.
    /// </summary>
    IEnumerable<EdgeSnapshot> VisibleEdges(float alphaThreshold = 0.02f);

    /// <summary>
    /// Computes center position of all nodes.
    /// </summary>
    Vec2? NodesCenter();

    /// <summary>
    /// Finds the node closest to the specified point.
    /// </summary>
    NodeSnapshot? ClosestNode(Vec2 point);

    /// <summary>
    /// Computes snapshot statistics.
    /// </summary>
    SceneStats ComputeStats();

    /// <summary>Total node count.</summary>
    int NodeCount { get; }

    /// <summary>Total avatar count.</summary>
    int AvatarCount { get; }

    /// <summary>Total edge count.</summary>
    int EdgeCount { get; }

    /// <summary>Total particle count.</summary>
    int ParticleCount { get; }

    /// <summary>Total object count.</summary>
    int TotalObjects { get; }

    /// <summary>True if the snapshot contains no objects.</summary>
    bool IsEmpty { get; }

    /// <summary>True if snapshot contains particles.</summary>
    bool HasParticles { get; }
}