using BenchmarkDotNet.Attributes;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Benchmarks per-frame scene simulation updates.
/// </summary>
/// <remarks>
/// Measures CPU-side work done before rendering: animation ticks, edge lifetime updates,
/// actor decay, camera updates, and layout stepping through <see cref="ChangeTrace.Rendering.Pipeline.SceneFrameUpdater"/>.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class SceneFrameUpdateBenchmarks
{
    private RenderBenchmarkFixture _fixture = null!;
    private double _wallElapsedSeconds;

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
    {
        _fixture = RenderBenchmarkFixture.Create(EventCount);
        _wallElapsedSeconds = 1.0;
    }

    /// <summary>
    /// Advances scene frame state by one simulated 60 FPS tick.
    /// </summary>
    [Benchmark]
    public float TickFrame()
    {
        _wallElapsedSeconds += 1.0 / 60.0;
        return _fixture.FrameUpdater.Tick(
            RenderBenchmarkFixture.CreateDiagnostics(EventCount, _wallElapsedSeconds));
    }
}
