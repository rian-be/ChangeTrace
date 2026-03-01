using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Player;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.States;

/// <summary>
/// Responsible for assembling immutable <see cref="RenderState"/> instances
/// from mutable rendering subsystems on each simulation tick.
/// </summary>
/// <remarks>
/// <para>
/// This class snapshots scene graph, animation system, camera, and diagnostics
/// into single immutable frame representation.
/// </para>
/// <para>
/// It also tracks per actor activity counts used to construct HUD leaderboard.
/// </para>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class RenderStateAssembler : IRenderStateAssembler
{
    private readonly Dictionary<string, int> _actorEventCounts = new();

    /// <summary>
    /// Records an event occurrence for specified actor.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <remarks>
    /// Used to compute leaderboard rankings in HUD snapshot.
    /// </remarks>
    public void RecordActorEvent(string actor)
        => _actorEventCounts[actor] = _actorEventCounts.GetValueOrDefault(actor) + 1;
    
    /// <summary>
    /// Creates new immutable <see cref="RenderState"/> snapshot for current tick.
    /// </summary>
    /// <param name="virtualTime">Current simulation time in virtual seconds.</param>
    /// <param name="wallDelta">Elapsed real-world time since previous frame.</param>
    /// <param name="scene">Mutable scene graph.</param>
    /// <param name="anim">Animation and particle system.</param>
    /// <param name="camera">Active camera instance.</param>
    /// <param name="diagnostics">Playback diagnostics and timing information.</param>
    /// <returns>A fully assembled <see cref="RenderState"/>.</returns>
    public RenderState Assemble(
        double virtualTime,
        double wallDelta,
        ISceneGraph scene,
        IAnimationSystem anim,
        Camera.Camera camera,
        PlayerDiagnostics diagnostics)
    {
        var sceneSnapshot= new SceneSnapshot(
            nodes: Snapshot(scene.Nodes.Values, static n => new NodeSnapshot(n.Id, n.Position, n.Radius, n.Color, n.Glow, n.Kind)),
            avatars: Snapshot(scene.Avatars.Values, static a => new AvatarSnapshot(a.Actor.Value, a.Position, a.Color, a.Alpha, a.ActivityLevel)),
            edges: Snapshot(scene.Edges, static e => new EdgeSnapshot(e.FromId, e.ToId, e.Kind, e.Alpha, ColorPalette.ForEdge(e.Kind))),
            particles: Snapshot(anim.Particles, static p => new ParticleSnapshot(p.Position, p.Alpha, p.Size, p.Color))
        );
        
        return new RenderState(
            VirtualTime:  virtualTime,
            WallDelta:    wallDelta,
            Progress:     diagnostics.Progress,
            CurrentSpeed: diagnostics.CurrentSpeed,
            Scene:        sceneSnapshot,
            Camera:       new CameraSnapshot(camera.Position, camera.Zoom, camera.Rotation),
            Hud:          BuildHud(diagnostics, sceneSnapshot));
    }
    
    /// <summary>
    /// Creates an immutable projection of collection.
    /// </summary>
    /// <typeparam name="TIn">Source element type.</typeparam>
    /// <typeparam name="TOut">Snapshot element type.</typeparam>
    /// <param name="source">Source collection.</param>
    /// <param name="projector">Projection function.</param>
    /// <returns>An immutable list containing projected elements.</returns>
    /// <remarks>
    /// If source implements <see cref="ICollection{T}"/>, capacity is preallocated
    /// for performance and allocation efficiency.
    /// </remarks>
    private static IReadOnlyList<TOut> Snapshot<TIn, TOut>(
        IEnumerable<TIn> source,
        Func<TIn, TOut> projector)
    {
        if (source is not ICollection<TIn> collection) return source.Select(projector).ToList();
        var result = new List<TOut>(collection.Count);
        result.AddRange(collection.Select(projector));
        return result;
    }
    
    private HudState BuildHud(PlayerDiagnostics diagnostics, SceneSnapshot snapshot)
    {
        var stats = snapshot.ComputeStats();

        return HudBuilder.Build(
            diagnostics,
            stats.ActiveAvatars,
            stats.NodeCount,
            BuildLeaderboard());
    }
    
    private LeaderboardEntry[] BuildLeaderboard()
        => _actorEventCounts
            .OrderByDescending(kv => kv.Value)
            .Take(10)
            .Select(kv => new LeaderboardEntry(kv.Key, kv.Value))
            .ToArray();

    public void Reset() => _actorEventCounts.Clear();
}