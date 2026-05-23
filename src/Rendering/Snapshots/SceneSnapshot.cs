using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Immutable snapshot of the current render scene state.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class SceneSnapshot : ISceneSnapshot
{
    /// <summary>
    /// Fast node lookup index.
    /// </summary>
    private readonly Dictionary<string, NodeSnapshot> _nodeIndex;

    internal SceneSnapshot(
        IReadOnlyList<NodeSnapshot> nodes,
        IReadOnlyList<AvatarSnapshot> avatars,
        IReadOnlyList<EdgeSnapshot> edges,
        IReadOnlyList<ParticleSnapshot> particles)
    {
        _nodeIndex =
            new Dictionary<string, NodeSnapshot>(
                nodes.Count);

        foreach (NodeSnapshot node in nodes)
            _nodeIndex[node.Id] = node;

        Nodes =
            _nodeIndex.Values
                .OrderBy(n => n.Kind switch
                {
                    NodeKind.Root => 0,
                    NodeKind.Branch => 1,
                    NodeKind.File => 2,
                    _ => 1
                })
                .ThenBy(n => n.Id)
                .ToArray();

        Avatars =
            avatars;

        Edges =
            edges
                .Where(e =>
                    !string.IsNullOrWhiteSpace(e.ToId) &&
                    !string.IsNullOrWhiteSpace(e.FromId) &&
                    _nodeIndex.ContainsKey(e.FromId) &&
                    _nodeIndex.ContainsKey(e.ToId))
                .DistinctBy(e =>
                    (e.FromId, e.ToId, e.Kind))
                .ToArray();

        Particles =
            particles;
    }

    /// <summary>
    /// Empty reusable scene snapshot.
    /// </summary>
    internal static SceneSnapshot Empty { get; } =
        new([], [], [], []);

    /// <summary>
    /// Scene nodes.
    /// </summary>
    public IReadOnlyList<NodeSnapshot> Nodes { get; }

    /// <summary>
    /// Scene avatars.
    /// </summary>
    public IReadOnlyList<AvatarSnapshot> Avatars { get; }

    /// <summary>
    /// Scene edges.
    /// </summary>
    public IReadOnlyList<EdgeSnapshot> Edges { get; }

    /// <summary>
    /// Active particles.
    /// </summary>
    public IReadOnlyList<ParticleSnapshot> Particles { get; }

    /// <summary>
    /// Total node count.
    /// </summary>
    public int NodeCount =>
        Nodes.Count;

    /// <summary>
    /// Total avatar count.
    /// </summary>
    public int AvatarCount =>
        Avatars.Count;

    /// <summary>
    /// Total edge count.
    /// </summary>
    public int EdgeCount =>
        Edges.Count;

    /// <summary>
    /// Total particle count.
    /// </summary>
    public int ParticleCount =>
        Particles.Count;

    /// <summary>
    /// Total render object count.
    /// </summary>
    public int TotalObjects =>
        NodeCount +
        AvatarCount +
        EdgeCount +
        ParticleCount;

    /// <summary>
    /// Indicates whether a snapshot contains no objects.
    /// </summary>
    public bool IsEmpty =>
        TotalObjects == 0;

    /// <summary>
    /// Indicates whether a snapshot contains particles.
    /// </summary>
    public bool HasParticles =>
        Particles.Count > 0;

    /// <summary>
    /// Finds node by identifier.
    /// </summary>
    public NodeSnapshot? FindNode(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return _nodeIndex.GetValueOrDefault(id);
    }

    /// <summary>
    /// Returns nodes matching the specified kind.
    /// </summary>
    public IEnumerable<NodeSnapshot> NodesOfKind(NodeKind kind) =>
        Nodes.Where(n => n.Kind == kind);

    /// <summary>
    /// Returns glowing nodes above a threshold.
    /// </summary>
    public IEnumerable<NodeSnapshot> GlowingNodes(
        float threshold = 0.05f) =>
        Nodes.Where(n => n.Glow > threshold);

    /// <summary>
    /// Returns active avatars above an activity threshold.
    /// </summary>
    public IEnumerable<AvatarSnapshot> ActiveAvatars(
        float activityThreshold = 0.1f) =>
        Avatars.Where(a => a.ActivityLevel > activityThreshold);

    /// <summary>
    /// Returns visible avatars above an alpha threshold.
    /// </summary>
    public IEnumerable<AvatarSnapshot> VisibleAvatars(
        float alphaThreshold = 0.05f) =>
        Avatars.Where(a =>
            a.Alpha * a.ActivityLevel > alphaThreshold);

    /// <summary>
    /// Finds avatar by actor identifier.
    /// </summary>
    public AvatarSnapshot? FindAvatar(string actor) =>
        Avatars.FirstOrDefault(a => a.Actor == actor);

    /// <summary>
    /// Returns outgoing edges for node.
    /// </summary>
    public IEnumerable<EdgeSnapshot> EdgesFrom(string nodeId) =>
        Edges.Where(e => e.FromId == nodeId);

    /// <summary>
    /// Returns incoming edges for node.
    /// </summary>
    public IEnumerable<EdgeSnapshot> EdgesTo(string nodeId) =>
        Edges.Where(e => e.ToId == nodeId);

    /// <summary>
    /// Returns edges matching a specified kind.
    /// </summary>
    public IEnumerable<EdgeSnapshot> EdgesOfKind(EdgeKind kind) =>
        Edges.Where(e => e.Kind == kind);

    /// <summary>
    /// Returns visible edges above an alpha threshold.
    /// </summary>
    public IEnumerable<EdgeSnapshot> VisibleEdges(
        float alphaThreshold = 0.02f) =>
        Edges.Where(e => e.Alpha > alphaThreshold);

    /// <summary>
    /// Computes average node center position.
    /// </summary>
    public Vec2? NodesCenter()
    {
        if (Nodes.Count == 0)
            return null;

        Vec2 sum =
            Vec2.Zero;

        foreach (NodeSnapshot node in Nodes)
            sum += node.Position;

        return sum / Nodes.Count;
    }

    /// <summary>
    /// Finds the closest node to the specified point.
    /// </summary>
    public NodeSnapshot? ClosestNode(Vec2 point)
    {
        if (Nodes.Count == 0)
            return null;

        NodeSnapshot? best =
            null;

        float bestDist =
            float.MaxValue;

        foreach (NodeSnapshot node in Nodes)
        {
            float dist =
                (node.Position - point).LengthSq;

            if (!(dist < bestDist))
                continue;

            bestDist = dist;
            best = node;
        }

        return best;
    }

    /// <summary>
    /// Computes aggregate scene statistics.
    /// </summary>
    public SceneStats ComputeStats() =>
        new(
            NodeCount: NodeCount,
            AvatarCount: AvatarCount,
            EdgeCount: EdgeCount,
            ParticleCount: ParticleCount,
            ActiveAvatars:
                Avatars.Count(a => a.ActivityLevel > 0.1f),
            GlowingNodes:
                Nodes.Count(n => n.Glow > 0.05f),
            VisibleEdges:
                Edges.Count(e => e.Alpha > 0.02f));
}