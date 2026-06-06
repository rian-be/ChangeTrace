using BenchmarkDotNet.Attributes;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Benchmarks CPU-side render state assembly.
/// </summary>
/// <remarks>
/// Measures the cost of converting live scene, animation, camera, HUD, and diagnostics state
/// into immutable <see cref="RenderState"/> snapshots before any OpenGL/GPU work starts.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class RenderStateAssemblyBenchmarks
{
    private RenderBenchmarkFixture _fixture = null!;

    /// <summary>
    /// Number of synthetic timeline events represented by the generated scene.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic render benchmark state for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
        => _fixture = RenderBenchmarkFixture.Create(EventCount);

    /// <summary>
    /// Assembles a complete immutable render state snapshot.
    /// </summary>
    [Benchmark]
    public int AssembleRenderState()
    {
        RenderState state = _fixture.AssembleRenderState();
        return state.Scene.TotalObjects;
    }
}
