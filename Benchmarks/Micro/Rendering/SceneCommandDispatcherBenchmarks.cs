using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Rendering;
using ChangeTrace.Benchmarks.Shared.Rendering;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Processors;
using ChangeTrace.Rendering.Processors.Handlers;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Benchmarks.Micro.Rendering;

/// <summary>
/// Benchmarks dispatch of translated render commands into scene handlers.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Rendering)]
public class SceneCommandDispatcherBenchmarks
{
    private RenderCommand[] _commands = null!;

    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    [GlobalSetup]
    public void Setup()
        => _commands = RenderingBenchmarkData.CreateRenderCommands(EventCount);

    [Benchmark]
    public int DispatchAllCommands()
    {
        var scene = new SceneGraph();
        var animation = new AnimationSystem();
        var assembler = new RenderStateAssembler();
        var dispatcher = new SceneCommandDispatcher(CreateHandlers(scene, animation, assembler));

        foreach (var command in _commands)
            dispatcher.Dispatch(command, command.Timestamp);

        return scene.Nodes.Count + scene.Edges.Count + animation.ParticleCount;
    }

    private static IRenderCommandHandler[] CreateHandlers(
        ISceneGraph scene,
        IAnimationSystem animation,
        IRenderStateAssembler assembler)
        =>
        [
            new FileNodeHandler(scene, animation, new NoopDiagnosticsProvider()),
            new MoveActorHandler(scene, animation, assembler),
            new BundledEdgeHandler(scene),
            new ParticleBurstHandler(scene, animation)
        ];

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
