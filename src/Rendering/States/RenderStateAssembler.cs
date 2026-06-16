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
using ChangeTrace.Rendering.States.Scene;
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

    private ISceneSnapshot _cachedSceneSnapshot = SceneSnapshot.Empty;
    private bool _hasCachedSceneSnapshot;

    /// <summary>
    /// Records contributor activity for leaderboard tracking.
    /// </summary>
    public void RecordActorEvent(string actor, string commitSha) =>
        _leaderboard.RecordActorEvent(actor);
    
    /// <summary>
    /// Clears accumulated render-related state.
    /// </summary>
    public void Reset() =>
        ResetCaches();

    private void ResetCaches()
    {
        _leaderboard.Reset();
        _extensions.Reset();
        _cachedSceneSnapshot = SceneSnapshot.Empty;
        _hasCachedSceneSnapshot = false;
    }

    private static Dictionary<string, int> BuildNodeIndex(
        IReadOnlyList<NodeSnapshot> nodes)
    {
        var nodeIndex = new Dictionary<string, int>(nodes.Count);

        for (var i = 0; i < nodes.Count; i++)
            nodeIndex[nodes[i].Id] = i;

        return nodeIndex;
    }

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
        LayoutMode layoutMode,
        bool sceneUnchanged = false)
    {
        int activeAvatarsCount =
            scene.Avatars.Count(static avatar => avatar.Value.ActivityLevel > 0.1f);

        var extensions = _extensions.Assemble(
            scene,
            sceneUnchanged);
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
            GetOrCreateSceneSnapshot(
                scene,
                animationSystem,
                sceneUnchanged);

        return new RenderState(
            virtualTime,
            wallDelta,
            sceneSnapshot,
            camera.ToSnapshot(),
            hudState,
            layoutMode,
            diagnostics.ManagedMemoryMb);
    }

    private ISceneSnapshot GetOrCreateSceneSnapshot(
        ISceneGraph scene,
        IAnimationSystem animationSystem,
        bool sceneUnchanged)
    {
        if (sceneUnchanged && _hasCachedSceneSnapshot)
            return _cachedSceneSnapshot;

        var nodeSnapshots = _nodes.Assemble(scene.Nodes);
        var nodeIndex = BuildNodeIndex(nodeSnapshots);
        var avatarSnapshots = _avatars.Assemble(scene.Avatars, out _);
        var edgeSnapshots = _edges.Assemble(scene, nodeIndex);
        var particleSnapshots = _particles.Assemble(animationSystem);

        _cachedSceneSnapshot =
            SceneSnapshotMaterializer.Create(
                nodeSnapshots,
                avatarSnapshots,
                edgeSnapshots,
                particleSnapshots);

        _hasCachedSceneSnapshot = true;
        return _cachedSceneSnapshot;
    }
}
