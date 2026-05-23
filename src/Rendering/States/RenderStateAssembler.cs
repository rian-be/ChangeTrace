using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Player;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States.Avatars;
using ChangeTrace.Rendering.States.Edges;
using ChangeTrace.Rendering.States.Hud;
using ChangeTrace.Rendering.States.Nodes;
using ChangeTrace.Rendering.States.Particles;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.States;

/// <summary>
/// Builds complete render state snapshots for the renderer pipeline.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class RenderStateAssembler : IRenderStateAssembler
{
    private readonly NodeSnapshotAssembler _nodes = new();
    private readonly AvatarSnapshotAssembler _avatars = new();
    private readonly EdgeSnapshotAssembler _edges = new();
    private readonly ParticleSnapshotAssembler _particles = new();

    private readonly ExtensionStatisticsAssembler _extensions = new();
    private readonly LeaderboardAssembler _leaderboard = new();
    private readonly HudStateAssembler _hud = new();

    /// <summary>
    /// Records contributor activity for leaderboard tracking.
    /// </summary>
    public void RecordActorEvent(string actor, string commitSha) =>
        _leaderboard.RecordActorEvent(actor);
    
    /// <summary>
    /// Clears accumulated render-related state.
    /// </summary>
    public void Reset() =>
        _leaderboard.Reset();

    /// <summary>
    /// Assembles a full immutable render state snapshot.
    /// </summary>
    public RenderState Assemble(
        double virtualTime,
        double wallDelta,
        ISceneGraph scene,
        IAnimationSystem animationSystem,
        Camera.Camera camera,
        ICameraController cameraController,
        PlayerDiagnostics diagnostics,
        SceneNode? hoveredNode,
        HoveredPodHud? hoveredPod,
        LayoutMode layoutMode)
    {
        var nodeSnapshots = _nodes.Assemble(scene.Nodes);
        var avatarSnapshots = _avatars.Assemble(scene.Avatars, out var activeAvatarsCount);
        var edgeSnapshots = _edges.Assemble(scene);
        var particleSnapshots = _particles.Assemble(animationSystem);
        var extensions = _extensions.Assemble(scene);
        var leaderboard = _leaderboard.Assemble();

        var hudState =
            _hud.Assemble(
                diagnostics,
                cameraController,
                layoutMode,
                hoveredNode,
                hoveredPod,
                activeAvatarsCount,
                scene.Nodes.Count,
                extensions,
                leaderboard);

        var sceneSnapshot =
            new SceneSnapshot(
                nodeSnapshots,
                avatarSnapshots,
                edgeSnapshots,
                particleSnapshots);

        return new RenderState(
            virtualTime,
            wallDelta,
            sceneSnapshot,
            camera.ToSnapshot(),
            hudState,
            layoutMode,
            diagnostics.ManagedMemoryMb);
    }
}