using System.Numerics;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Models;
using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Camera;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Pipeline;
using ChangeTrace.Rendering.Processors;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using ChangeTrace.Rendering.States.Avatars;
using ChangeTrace.Rendering.States.Edges;
using ChangeTrace.Rendering.States.Nodes;
using ChangeTrace.Rendering.States.Particles;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Shared deterministic fixture for CPU-side rendering benchmarks.
/// </summary>
/// <remarks>
/// Creates a synthetic scene graph, animation system, camera, diagnostics snapshot,
/// and frame pipeline components without opening OpenTK windows or requiring GPU access.
/// </remarks>
internal sealed class RenderBenchmarkFixture
{
    private readonly RenderStateAssembler _stateAssembler;
    private readonly NodeSnapshotAssembler _nodeSnapshots = new();
    private readonly AvatarSnapshotAssembler _avatarSnapshots = new();
    private readonly EdgeSnapshotAssembler _edgeSnapshots = new();
    private readonly ParticleSnapshotAssembler _particleSnapshots = new();

    private RenderBenchmarkFixture(
        int eventCount,
        SceneGraph scene,
        AnimationSystem animation,
        Camera camera,
        CameraController cameraController,
        RenderStateAssembler stateAssembler,
        RenderFrameAssembler frameAssembler,
        SceneFrameUpdater frameUpdater,
        PlayerDiagnostics diagnostics)
    {
        EventCount = eventCount;
        Scene = scene;
        Animation = animation;
        Camera = camera;
        CameraController = cameraController;
        FrameAssembler = frameAssembler;
        FrameUpdater = frameUpdater;
        Diagnostics = diagnostics;
        _stateAssembler = stateAssembler;
    }

    public int EventCount { get; }

    /// <summary>
    /// Synthetic scene graph populated with root, branch, file, and avatar nodes.
    /// </summary>
    public SceneGraph Scene { get; }

    /// <summary>
    /// Animation and particle system used by render snapshot assembly.
    /// </summary>
    public AnimationSystem Animation { get; }

    /// <summary>
    /// Mutable camera state used by render state assembly.
    /// </summary>
    public Camera Camera { get; }

    /// <summary>
    /// Camera controller used by HUD and scene frame updates.
    /// </summary>
    public CameraController CameraController { get; }

    /// <summary>
    /// Frame assembler under benchmark.
    /// </summary>
    public RenderFrameAssembler FrameAssembler { get; }

    /// <summary>
    /// Scene frame updater under benchmark.
    /// </summary>
    public SceneFrameUpdater FrameUpdater { get; }

    /// <summary>
    /// Baseline player diagnostics snapshot used by render assembly.
    /// </summary>
    public PlayerDiagnostics Diagnostics { get; }

    /// <summary>
    /// Builds a complete fixture for the requested synthetic event count.
    /// </summary>
    /// <param name="eventCount">Synthetic timeline event count represented by the scene.</param>
    public static RenderBenchmarkFixture Create(int eventCount)
    {
        var scene = BenchmarkSceneFactory.Create(eventCount);
        var animation = BenchmarkSceneFactory.CreateAnimation(eventCount);
        var camera = new Camera();
        var cameraController = new CameraController(camera);
        var stateAssembler = new RenderStateAssembler();
        var frameAssembler = new RenderFrameAssembler(stateAssembler, new BenchmarkRenderOutput());
        var diagnostics = CreateDiagnostics(eventCount, wallElapsedSeconds: 1.0);
        var frameUpdater = new SceneFrameUpdater(
            scene,
            animation,
            new NoopLayoutEngine(),
            cameraController,
            new ActorDecaySystem(new NoopDiagnosticsProvider()),
            viewportSize: new Vec2(1920, 1080));

        return new RenderBenchmarkFixture(
            eventCount,
            scene,
            animation,
            camera,
            cameraController,
            stateAssembler,
            frameAssembler,
            frameUpdater,
            diagnostics);
    }

    /// <summary>
    /// Assembles a render state snapshot using the fixture state.
    /// </summary>
    public RenderState AssembleRenderState()
        => _stateAssembler.Assemble(
            virtualTime: 1.0,
            wallDelta: 1.0 / 60.0,
            Scene,
            Animation,
            Camera,
            CameraController,
            Diagnostics,
            hoveredNode: null,
            hoveredPod: null,
            LayoutMode.SingleTree);

    /// <summary>
    /// Assembles only the immutable scene snapshot without HUD or camera state.
    /// </summary>
    public SceneSnapshot AssembleSceneSnapshot()
    {
        var nodes = _nodeSnapshots.Assemble(Scene.Nodes);
        var avatars = _avatarSnapshots.Assemble(Scene.Avatars, out _);
        var edges = _edgeSnapshots.Assemble(Scene);
        var particles = _particleSnapshots.Assemble(Animation);

        return new SceneSnapshot(nodes, avatars, edges, particles);
    }

    /// <summary>
    /// Creates deterministic playback diagnostics for benchmark frame simulation.
    /// </summary>
    /// <param name="eventCount">Synthetic timeline event count.</param>
    /// <param name="wallElapsedSeconds">Simulated wall-clock playback time.</param>
    public static PlayerDiagnostics CreateDiagnostics(
        int eventCount,
        double wallElapsedSeconds)
        => new(
            State: PlayerState.Playing,
            Mode: PlaybackMode.Once,
            Direction: PlaybackDirection.Forward,
            CurrentSpeed: 1.0,
            TargetSpeed: 1.0,
            IsRamping: false,
            PositionSeconds: wallElapsedSeconds,
            DurationSeconds: 100.0,
            Progress: wallElapsedSeconds / 100.0,
            EventsFired: Math.Min(eventCount, (int)(wallElapsedSeconds * 1000)),
            TotalEvents: eventCount,
            LoopCount: 0,
            WallElapsedSeconds: wallElapsedSeconds,
            TickCount: (int)(wallElapsedSeconds * 60.0),
            AvgEventsPerTick: eventCount / 60.0);

    /// <summary>
    /// Creates deterministic semantic commit bundle events used to benchmark render translation.
    /// </summary>
    /// <param name="eventCount">Number of synthetic commit bundle events to create.</param>
    public static CommitBundleEvent[] CreateCommitBundles(int eventCount)
    {
        var events = new CommitBundleEvent[eventCount];

        for (var i = 0; i < eventCount; i++)
        {
            var fileCount = 1 + i % 5;
            var files = new string[fileCount];

            for (var fileIndex = 0; fileIndex < fileCount; fileIndex++)
            {
                files[fileIndex] =
                    $"src/module-{i % 128}/feature-{fileIndex}/file-{i}-{fileIndex}.cs";
            }

            events[i] = new CommitBundleEvent(
                commitSha: $"commit-{i:x8}",
                actor: $"actor-{i % 128}",
                timestamp: i,
                files);
        }

        return events;
    }

    private sealed class BenchmarkRenderOutput : IRenderOutput
    {
        /// <inheritdoc />
        public void Initialize(int width, int height)
        {
        }

        /// <inheritdoc />
        public void Resize(int width, int height)
        {
        }

        /// <inheritdoc />
        public void Submit(RenderState state)
        {
        }
    }

    private sealed class NoopLayoutEngine : ILayoutEngine
    {
        /// <inheritdoc />
        public float Energy => 0.0f;

        /// <inheritdoc />
        public void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds)
        {
        }
    }

    private sealed class NoopDiagnosticsProvider : IDiagnosticsProvider
    {
        /// <inheritdoc />
        public MemoryMetrics GetMemoryMetrics()
            => new(0, 0, 0);

        /// <inheritdoc />
        public int[] GetGcCollections()
            => [0, 0, 0];

        /// <inheritdoc />
        public RuntimeMetrics GetRuntimeMetrics()
            => new(0, 0, 0, 0);

        /// <inheritdoc />
        public IReadOnlyDictionary<string, double> GetCustomMetrics()
            => new Dictionary<string, double>();

        /// <inheritdoc />
        public void RecordMetric(string key, double value)
        {
        }

        /// <inheritdoc />
        public void RecordEvent(string category, string label)
        {
        }

        /// <inheritdoc />
        public IReadOnlyList<KeyValuePair<string, int>> GetTopEvents(string category, int count)
            => [];
    }
}

/// <summary>
/// Creates synthetic render scenes for benchmark scenarios.
/// </summary>
/// <remarks>
/// Scene sizes scale with the benchmark event count and are deterministic enough for
/// repeated local comparisons while still exercising node, avatar, particle, and HUD paths.
/// </remarks>
internal static class BenchmarkSceneFactory
{
    /// <summary>
    /// Creates a populated scene graph representing a repository at the requested scale.
    /// </summary>
    /// <param name="eventCount">Synthetic timeline event count used to scale scene size.</param>
    public static SceneGraph Create(int eventCount)
    {
        var scene = new SceneGraph();
        scene.GetOrCreateRoot();

        var nodeCount = Math.Clamp(eventCount, 1_000, 100_000);
        var branchCount = Math.Clamp(nodeCount / 250, 4, 400);

        for (var branchIndex = 0; branchIndex < branchCount; branchIndex++)
        {
            var position = PositionOnCircle(branchIndex, branchCount, radius: 900);
            scene.GetOrAddNode($"branch-{branchIndex}", NodeKind.Branch, position);
        }

        for (var i = 0; i < nodeCount; i++)
        {
            var branchIndex = i % branchCount;
            var layer = i % 40;
            var position = PositionOnCircle(i, nodeCount, radius: 1200 + layer * 4);
            var node = scene.GetOrAddNode(
                $"src/module-{branchIndex}/feature-{i % 64}/file-{i}.cs",
                NodeKind.File,
                position);

            node.Glow = (i % 10) / 10.0f;
            node.LastAuthor = $"actor-{i % 128}";
            node.LastCommit = $"commit-{i}";
        }

        var avatarCount = Math.Clamp(eventCount / 100, 10, 1_000);
        for (var i = 0; i < avatarCount; i++)
        {
            var actor = ActorName.Create($"actor-{i}").Value;
            var avatar = scene.GetOrAddAvatar(
                actor,
                PositionOnCircle(i, avatarCount, radius: 600),
                new Vector4((i % 17) / 16.0f, (i % 23) / 22.0f, (i % 31) / 30.0f, 1.0f));

            avatar.ActivityLevel = 1.0f;
            avatar.LastSeen = i;
            avatar.TargetNodeId = $"src/module-{i % branchCount}/feature-{i % 64}/file-{i % nodeCount}.cs";
        }

        return scene;
    }

    /// <summary>
    /// Creates an animation system with a bounded number of particle bursts.
    /// </summary>
    /// <param name="eventCount">Synthetic timeline event count used to scale particle bursts.</param>
    public static AnimationSystem CreateAnimation(int eventCount)
    {
        var animation = new AnimationSystem();
        var burstCount = Math.Clamp(eventCount / 1_000, 1, 20);

        for (var i = 0; i < burstCount; i++)
        {
            animation.Burst(
                PositionOnCircle(i, burstCount, radius: 300),
                count: 25,
                color: new Vector4(0.1f, 0.7f, 1.0f, 1.0f));
        }

        return animation;
    }

    private static Vec2 PositionOnCircle(int index, int count, float radius)
    {
        var angle = MathF.Tau * index / count;
        return new Vec2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);
    }
}
