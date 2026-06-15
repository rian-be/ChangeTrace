using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Shared.Rendering;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Results;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Interfaces;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Camera;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Pipeline;
using ChangeTrace.Rendering.Processors.Handlers;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.States;
using ChangeTrace.Rendering.Translators;

namespace ChangeTrace.Benchmarks.Subsystem.Rendering;

/// <summary>
/// Benchmarks buffering, semantic aggregation, and flush into the rendering pipeline.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Rendering)]
public class RenderEventBufferBenchmarks
{
    private TraceEvent[] _events = null!;

    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    [GlobalSetup]
    public void Setup()
        => _events = RenderingBenchmarkData.CreateTraceEvents(EventCount);

    [Benchmark]
    public int AddAllEventsAndFlush()
    {
        var scene = new SceneGraph();
        var animation = new AnimationSystem();
        var camera = new Camera();
        var cameraController = new CameraController(camera);
        var assembler = new RenderStateAssembler();
        var diagnostics = new NoopDiagnosticsProvider();
        using var buffer = new RenderEventBuffer(RenderEventKinds.Commit);
        using var pipeline = new RenderingPipeline(
            new NoopTimelinePlayer(),
            new NoopRenderOutput(),
            new NoopLayoutEngine(),
            camera,
            cameraController,
            scene,
            animation,
            TranslationPipeline.Default(),
            assembler,
            [
                new FileNodeHandler(scene, animation, diagnostics),
                new MoveActorHandler(scene, animation, assembler),
                new BundledEdgeHandler(scene),
                new ParticleBurstHandler(scene, animation)
            ],
            viewportSize: new Vec2(1920, 1080),
            diagnostics);

        foreach (var evt in _events)
            buffer.Add(evt);

        buffer.FlushTo(pipeline);
        return scene.Nodes.Count + scene.Edges.Count + animation.ParticleCount;
    }

    private sealed class NoopLayoutEngine : ILayoutEngine
    {
        public float Energy => 0f;
        public void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds) { }
    }

    private sealed class NoopRenderOutput : IRenderOutput
    {
        public void Initialize(int width, int height) { }
        public void Resize(int width, int height) { }
        public void Submit(RenderState state) { }
    }

    private sealed class NoopTimelinePlayer : ITimelinePlayer
    {
        public double DurationSeconds => 0;
        public PlayerState State => PlayerState.Idle;
        public PlaybackDirection Direction => PlaybackDirection.Forward;
        public PlaybackMode Mode { get; set; } = PlaybackMode.Once;
        public double CurrentSpeed => 1;
        public double TargetSpeed { get; set; } = 1;
        public double Acceleration { get; set; } = 1;
        public double PositionSeconds => 0;
        public double Progress => 0;
        public event Action<TraceEvent>? OnEvent;
        public event Action<PlayerState>? OnStateChanged;
        public event Action<double>? OnProgress;
        public event Action<int>? OnLoopCompleted;
        public Result Play() => Result.Success();
        public Result Pause() => Result.Success();
        public Result Stop() => Result.Success();
        public Result Seek(ChangeTrace.Core.Models.Timestamp position) => Result.Success();
        public Result SeekRelative(double deltaSeconds) => Result.Success();
        public Result StepForward() => Result.Failure("noop");
        public Result StepBackward() => Result.Failure("noop");
        public Result ApplyPreset(SpeedPreset preset) => Result.Success();
        public ChangeTrace.Player.PlayerDiagnostics GetDiagnostics() => RenderBenchmarkFixture.CreateDiagnostics(1, 0);
        public void Dispose() { }
    }

    private sealed class NoopDiagnosticsProvider : IDiagnosticsProvider
    {
        public MemoryMetrics GetMemoryMetrics() => new(0, 0, 0);
        public int[] GetGcCollections() => [0, 0, 0];
        public RuntimeMetrics GetRuntimeMetrics() => new(0, 0, 0, 0);
        public IReadOnlyDictionary<string, double> GetCustomMetrics() => new Dictionary<string, double>();
        public void RecordMetric(string key, double value) { }
        public void RecordEvent(string category, string label) { }
        public IReadOnlyList<KeyValuePair<string, int>> GetTopEvents(string category, int count) => [];
    }
}
