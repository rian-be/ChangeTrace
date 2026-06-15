using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Info;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.Player;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Player.Playback;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Camera;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Pipeline;
using ChangeTrace.Rendering.Processors.Handlers;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.States;
using ChangeTrace.Rendering.Translators;

namespace ChangeTrace.Benchmarks.Shared.Rendering;

/// <summary>
/// Shared deterministic fixture for player-driven rendering benchmarks.
/// </summary>
/// <remarks>
/// Creates a real <see cref="TimelinePlayer"/> and wires it into <see cref="RenderingPipeline"/>
/// so benchmarks cover event aggregation, translation, scene mutation, frame update, and frame submission
/// without opening windows or touching GPU drivers.
/// </remarks>
internal sealed class PlayerRenderBenchmarkFixture
{
    private PlayerRenderBenchmarkFixture(
        int eventCount,
        Timeline timeline)
    {
        EventCount = eventCount;
        Timeline = timeline;
    }

    /// <summary>
    /// Number of synthetic trace events represented by the fixture.
    /// </summary>
    public int EventCount { get; }

    /// <summary>
    /// Synthetic timeline used by the player-driven render pipeline.
    /// </summary>
    public Timeline Timeline { get; }

    /// <summary>
    /// Builds a fixture with synthetic commit-style trace events.
    /// </summary>
    public static PlayerRenderBenchmarkFixture Create(int eventCount)
    {
        var timeline = CreateTimeline(eventCount);
        TimelineNormalizer.Normalize(timeline, targetDurationSeconds: eventCount);
        return new PlayerRenderBenchmarkFixture(eventCount, timeline);
    }

    /// <summary>
    /// Creates a fresh player/render harness with manual transport ticking.
    /// </summary>
    public PlayerRenderBenchmarkHarness CreateHarness()
    {
        var scene = new SceneGraph();
        var animation = new AnimationSystem();
        var camera = new Camera();
        var cameraController = new CameraController(camera);
        var assembler = new RenderStateAssembler();
        var output = new BenchmarkRenderOutput();
        var layout = new NoopLayoutEngine();
        var translation = TranslationPipeline.Default();

        var diagnostics = new NoopDiagnosticsProvider();
        var handlers = CreateHandlers(scene, animation, assembler, diagnostics);

        var clock = new VirtualClock(initialSpeed: 1.0, acceleration: 1.0);
        var cursor = new EventCursor(Timeline.Events);
        var seekable = new SeekableTimeline(clock, cursor, EventCount);
        var transport = new ManualPlaybackTransport { Mode = PlaybackMode.Once };
        var player = new TimelinePlayer(clock, cursor, seekable, transport, diagnostics, PlaybackMode.Once);

        var pipeline = new RenderingPipeline(
            player,
            output,
            layout,
            camera,
            cameraController,
            scene,
            animation,
            translation,
            assembler,
            handlers,
            viewportSize: new Vec2(1920, 1080),
            diagnostics);

        return new PlayerRenderBenchmarkHarness(
            player,
            pipeline,
            clock,
            transport,
            scene,
            animation,
            output);
    }

    private static Timeline CreateTimeline(int eventCount)
    {
        var repository = RepositoryId.Create("bench", "player-render").Value;
        var timeline = new Timeline(repository, eventCount);
        const long baseUnix = 1_700_000_000;

        var eventIndex = 0;
        var commitIndex = 0;

        while (eventIndex < eventCount)
        {
            var remaining = eventCount - eventIndex;
            var fileEvents = Math.Min(3, Math.Max(0, remaining - 1));
            var sha = CommitSha.Create($"{commitIndex + 0x1000000:x7}").Value;
            var actor = ActorName.Create($"actor-{commitIndex % 128}").Value;

            for (var fileIndex = 0; fileIndex < fileEvents; fileIndex++)
            {
                var timestamp = Timestamp.Create(baseUnix + eventIndex).Value;
                var filePath =
                    $"src/module-{commitIndex % 64}/feature-{fileIndex}/file-{commitIndex}-{fileIndex}.cs";

                timeline.AddEvent(new TraceEvent(
                    new TraceEventCore(timestamp, actor, filePath),
                    Commit: new CommitInfo(sha, FileChangeKind.Modified)));

                eventIndex++;
            }

            if (eventIndex < eventCount)
            {
                var timestamp = Timestamp.Create(baseUnix + eventIndex).Value;
                var markerPath =
                    $"src/module-{commitIndex % 64}/commit-{commitIndex}.cs";

                timeline.AddEvent(new TraceEvent(
                    new TraceEventCore(timestamp, actor, markerPath),
                    Commit: new CommitInfo(sha, FileChangeKind.Commit)));

                eventIndex++;
            }

            commitIndex++;
        }

        return timeline;
    }

    private static IRenderCommandHandler[] CreateHandlers(
        ISceneGraph scene,
        IAnimationSystem animation,
        IRenderStateAssembler assembler,
        IDiagnosticsProvider diagnostics)
        =>
        [
            new FileNodeHandler(scene, animation, diagnostics),
            new MoveActorHandler(scene, animation, assembler),
            new BundledEdgeHandler(scene),
            new ParticleBurstHandler(scene, animation)
        ];

    internal sealed class PlayerRenderBenchmarkHarness(
        TimelinePlayer player,
        RenderingPipeline pipeline,
        VirtualClock clock,
        ManualPlaybackTransport transport,
        SceneGraph scene,
        AnimationSystem animation,
        BenchmarkRenderOutput output)
        : IDisposable
    {
        public TimelinePlayer Player { get; } = player;

        public RenderingPipeline Pipeline { get; } = pipeline;

        public VirtualClock Clock { get; } = clock;

        public ManualPlaybackTransport Transport { get; } = transport;

        public SceneGraph Scene { get; } = scene;

        public AnimationSystem Animation { get; } = animation;

        public BenchmarkRenderOutput Output { get; } = output;

        public void Dispose()
        {
            Pipeline.Dispose();
            Player.Dispose();
        }
    }

    internal sealed class ManualPlaybackTransport : IPlaybackTransport
    {
        public PlayerState State { get; private set; } = PlayerState.Idle;

        public PlaybackMode Mode { get; set; } = PlaybackMode.Once;

        public event Action<PlayerState>? OnStateChanged;

        public event Action? OnTick;

        public Result Play()
        {
            State = PlayerState.Playing;
            OnStateChanged?.Invoke(State);
            return Result.Success();
        }

        public Result Pause()
        {
            if (State != PlayerState.Playing)
                return Result.Failure($"Cannot pause while {State}.");

            State = PlayerState.Paused;
            OnStateChanged?.Invoke(State);
            return Result.Success();
        }

        public Result Stop()
        {
            State = PlayerState.Idle;
            OnStateChanged?.Invoke(State);
            return Result.Success();
        }

        public void TriggerTick()
            => OnTick?.Invoke();

        public void Dispose()
        {
        }
    }

    internal sealed class BenchmarkRenderOutput : IRenderOutput
    {
        public int SubmittedFrames { get; private set; }

        public int LastObjectCount { get; private set; }

        public void Initialize(int width, int height)
        {
        }

        public void Resize(int width, int height)
        {
        }

        public void Submit(RenderState state)
        {
            SubmittedFrames++;
            LastObjectCount = state.Scene.TotalObjects;
        }
    }

    private sealed class NoopLayoutEngine : ILayoutEngine
    {
        public float Energy => 0.0f;

        public void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds)
        {
        }
    }

    private sealed class NoopDiagnosticsProvider : IDiagnosticsProvider
    {
        public MemoryMetrics GetMemoryMetrics()
            => new(0, 0, 0);

        public int[] GetGcCollections()
            => [0, 0, 0];

        public RuntimeMetrics GetRuntimeMetrics()
            => new(0, 0, 0, 0);

        public IReadOnlyDictionary<string, double> GetCustomMetrics()
            => new Dictionary<string, double>();

        public void RecordMetric(string key, double value)
        {
        }

        public void RecordEvent(string category, string label)
        {
        }

        public IReadOnlyList<KeyValuePair<string, int>> GetTopEvents(string category, int count)
            => [];
    }
}
