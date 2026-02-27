using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Snapshots;

/// <summary>
/// Represents snapshot of scene at specific moment in virtual time.
/// </summary>
/// <remarks>
/// Holds immutable lists of nodes, avatars, edges, and particles.
/// Provides helper methods for filtering, spatial queries, and basic statistics.
/// </remarks>
internal sealed class SceneSnapshot : ISceneSnapshot
{
    private readonly IReadOnlyList<NodeSnapshot> _nodes;
    private readonly IReadOnlyList<AvatarSnapshot> _avatars;
    private readonly IReadOnlyList<EdgeSnapshot> _edges;
    private readonly IReadOnlyList<ParticleSnapshot> _particles;

    private Dictionary<string, NodeSnapshot>? _nodeIndex;

    internal SceneSnapshot(
        IReadOnlyList<NodeSnapshot> nodes,
        IReadOnlyList<AvatarSnapshot> avatars,
        IReadOnlyList<EdgeSnapshot> edges,
        IReadOnlyList<ParticleSnapshot> particles)
    {
        _nodes = nodes;
        _avatars = avatars;
        _edges = edges;
        _particles = particles;
    }

    /// <summary>
    /// An empty scene snapshot with no nodes, avatars, edges, or particles.
    /// </summary>
    internal static SceneSnapshot Empty { get; } = new([], [], [], []);

    public IReadOnlyList<NodeSnapshot> Nodes => _nodes;
    public IReadOnlyList<AvatarSnapshot> Avatars => _avatars;
    public IReadOnlyList<EdgeSnapshot> Edges => _edges;
    public IReadOnlyList<ParticleSnapshot> Particles => _particles;

    public int NodeCount => _nodes.Count;
    public int AvatarCount => _avatars.Count;
    public int EdgeCount => _edges.Count;
    public int ParticleCount => _particles.Count;
    public int TotalObjects => NodeCount + AvatarCount + EdgeCount + ParticleCount;

    public bool IsEmpty => TotalObjects == 0;
    public bool HasParticles => _particles.Count > 0;

    /// <summary>
    /// Finds node by its identifier.
    /// </summary>
    /// <param name="id">The node's unique identifier.</param>
    /// <returns>The <see cref="NodeSnapshot"/> if found; otherwise, null.</returns>
    public NodeSnapshot? FindNode(string id)
    {
        _nodeIndex ??= _nodes.ToDictionary(n => n.Id);
        return _nodeIndex.GetValueOrDefault(id);
    }

    /// <summary>
    /// Returns nodes of given kind.
    /// </summary>
    /// <param name="kind">The kind of nodes to filter.</param>
    /// <returns>An enumerable of <see cref="NodeSnapshot"/> objects.</returns>
    public IEnumerable<NodeSnapshot> NodesOfKind(NodeKind kind)
        => _nodes.Where(n => n.Kind == kind);

    /// <summary>
    /// Returns nodes with glow above threshold.
    /// </summary>
    /// <param name="threshold">Glow threshold.</param>
    /// <returns>An enumerable of <see cref="NodeSnapshot"/> objects.</returns>
    public IEnumerable<NodeSnapshot> GlowingNodes(float threshold = 0.05f)
        => _nodes.Where(n => n.Glow > threshold);

    /// <summary>
    /// Returns avatars with activity above threshold.
    /// </summary>
    /// <param name="activityThreshold">Activity threshold.</param>
    /// <returns>An enumerable of <see cref="AvatarSnapshot"/> objects.</returns>
    public IEnumerable<AvatarSnapshot> ActiveAvatars(float activityThreshold = 0.1f)
        => _avatars.Where(a => a.ActivityLevel > activityThreshold);

    /// <summary>
    /// Returns avatars with alpha above threshold.
    /// </summary>
    /// <param name="alphaThreshold">Alpha threshold.</param>
    /// <returns>An enumerable of <see cref="AvatarSnapshot"/> objects.</returns>
    public IEnumerable<AvatarSnapshot> VisibleAvatars(float alphaThreshold = 0.05f)
        => _avatars.Where(a => a.Alpha > alphaThreshold);

    /// <summary>
    /// Finds an avatar by actor name.
    /// </summary>
    /// <param name="actor">The actor's name.</param>
    /// <returns>The <see cref="AvatarSnapshot"/> if found; otherwise, null.</returns>
    public AvatarSnapshot? FindAvatar(string actor)
        => _avatars.FirstOrDefault(a => a.Actor == actor);

    /// <summary>
    /// Returns edges starting from given node.
    /// </summary>
    /// <param name="nodeId">The source node identifier.</param>
    /// <returns>An enumerable of <see cref="EdgeSnapshot"/> objects.</returns>
    public IEnumerable<EdgeSnapshot> EdgesFrom(string nodeId)
        => _edges.Where(e => e.FromId == nodeId);

    /// <summary>
    /// Returns edges ending at given node.
    /// </summary>
    /// <param name="nodeId">The target node identifier.</param>
    /// <returns>An enumerable of <see cref="EdgeSnapshot"/> objects.</returns>
    public IEnumerable<EdgeSnapshot> EdgesTo(string nodeId)
        => _edges.Where(e => e.ToId == nodeId);

    /// <summary>
    /// Returns edges of specified kind.
    /// </summary>
    /// <param name="kind">The kind of edge.</param>
    /// <returns>An enumerable of <see cref="EdgeSnapshot"/> objects.</returns>
    public IEnumerable<EdgeSnapshot> EdgesOfKind(EdgeKind kind)
        => _edges.Where(e => e.Kind == kind);

    /// <summary>
    /// Returns edges with alpha above threshold.
    /// </summary>
    /// <param name="alphaThreshold">Alpha threshold.</param>
    /// <returns>An enumerable of <see cref="EdgeSnapshot"/> objects.</returns>
    public IEnumerable<EdgeSnapshot> VisibleEdges(float alphaThreshold = 0.02f)
        => _edges.Where(e => e.Alpha > alphaThreshold);

    /// <summary>
    /// Calculates axis aligned bounding box of all nodes, or null if there are no nodes.
    /// </summary>
    /// <returns>A <see cref="Bounds"/> object or null.</returns>
    public Bounds? NodeBounds()
    {
        if (_nodes.Count == 0) return null;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var n in _nodes)
        {
            minX = MathF.Min(minX, n.Position.X);
            maxX = MathF.Max(maxX, n.Position.X);
            minY = MathF.Min(minY, n.Position.Y);
            maxY = MathF.Max(maxY, n.Position.Y);
        }

        return new Bounds(new Vec2(minX, minY), new Vec2(maxX, maxY));
    }

    /// <summary>
    ///  Geometric center of all nodes, or null if none exist.
    /// </summary>
    /// <returns>The center position as <see cref="Vec2"/>; or null.</returns>
    public Vec2? NodesCenter()
    {
        if (_nodes.Count == 0) return null;
        var sum = Vec2.Zero;
        foreach (var n in _nodes) sum += n.Position;
        return sum / _nodes.Count;
    }

    /// <summary>
    /// Finds node closest to given point.
    /// </summary>
    /// <param name="point">The reference point in world coordinates.</param>
    /// <returns>The closest <see cref="NodeSnapshot"/>; or null if no nodes exist.</returns>
    public NodeSnapshot? ClosestNode(Vec2 point)
    {
        if (_nodes.Count == 0) return null;
        NodeSnapshot? best = null;
        float bestDist = float.MaxValue;

        foreach (var n in _nodes)
        {
            float dist = (n.Position - point).LengthSq;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = n;
            }
        }

        return best;
    }

    /// <summary>
    /// Returns basic statistics about scene.
    /// </summary>
    /// <returns>A <see cref="SceneStats"/> object.</returns>
    public SceneStats ComputeStats() => new(
        NodeCount: NodeCount,
        AvatarCount: AvatarCount,
        EdgeCount: EdgeCount,
        ParticleCount: ParticleCount,
        ActiveAvatars: _avatars.Count(a => a.ActivityLevel > 0.1f),
        GlowingNodes: _nodes.Count(n => n.Glow > 0.05f),
        VisibleEdges: _edges.Count(e => e.Alpha > 0.02f)
    );
}