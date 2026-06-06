using BenchmarkDotNet.Attributes;
using ChangeTrace.Rendering.States;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Benchmarks CPU-side preparation of data shaped like GPU buffer contracts.
/// </summary>
/// <remarks>
/// Measures conversion from immutable render snapshots into arrays of GPU contract structs
/// such as nodes, actor pawns, and edges. This does not upload data to OpenGL.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class GpuBufferDataPreparationBenchmarks
{
    private RenderState _state = null!;

    /// <summary>
    /// Number of synthetic timeline events represented by the generated scene.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates a prepared render state used as input for GPU data conversion.
    /// </summary>
    [GlobalSetup]
    public void Setup()
        => _state = RenderBenchmarkFixture.Create(EventCount).AssembleRenderState();

    /// <summary>
    /// Converts render snapshots into CPU arrays matching GPU-side contracts.
    /// </summary>
    [Benchmark]
    public int PrepareGpuBufferData()
    {
        var data = GpuBufferData.From(_state);
        return data.TotalCount;
    }
}
