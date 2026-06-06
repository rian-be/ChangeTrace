using BenchmarkDotNet.Attributes;
using ChangeTrace.Rendering.Layout.Hive.Core;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Benchmarks CPU-side hive layout computation.
/// </summary>
/// <remarks>
/// Measures repository tree indexing, directory placement, heavy file cluster layout,
/// normalization, and animated node movement without scene snapshot or OpenGL work.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class HiveLayoutBenchmarks
{
    private RenderBenchmarkFixture _fixture = null!;
    private HiveLayout _layout = null!;

    /// <summary>
    /// Number of synthetic timeline events represented by the generated scene.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic scene state and a fresh hive layout engine.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _fixture = RenderBenchmarkFixture.Create(EventCount);
        _layout = new HiveLayout();
    }

    /// <summary>
    /// Runs one hive layout step for the current scene.
    /// </summary>
    [Benchmark]
    public float StepOnce()
    {
        _layout.Step(
            _fixture.Scene.Nodes,
            deltaSeconds: 1.0f / 60.0f);

        return _layout.Energy;
    }

    /// <summary>
    /// Runs several consecutive hive layout steps to include animated convergence cost.
    /// </summary>
    [Benchmark]
    public float StepTenFrames()
    {
        for (var i = 0; i < 10; i++)
        {
            _layout.Step(
                _fixture.Scene.Nodes,
                deltaSeconds: 1.0f / 60.0f);
        }

        return _layout.Energy;
    }
}
