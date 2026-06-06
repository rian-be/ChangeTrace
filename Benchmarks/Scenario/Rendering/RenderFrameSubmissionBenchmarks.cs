using BenchmarkDotNet.Attributes;
using ChangeTrace.Rendering;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Benchmarks the frame submission facade used by the renderer.
/// </summary>
/// <remarks>
/// Covers <see cref="ChangeTrace.Rendering.Pipeline.RenderFrameAssembler"/> overhead,
/// including render state assembly and submission to an <see cref="ChangeTrace.Rendering.Interfaces.IRenderOutput"/>.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class RenderFrameSubmissionBenchmarks
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
    /// Submits one render frame to the benchmark output sink.
    /// </summary>
    [Benchmark]
    public void SubmitFrame()
    {
        _fixture.FrameAssembler.SubmitFrame(
            positionSeconds: 1.0,
            deltaTime: 1.0f / 60.0f,
            _fixture.Scene,
            _fixture.Animation,
            _fixture.Camera,
            _fixture.CameraController,
            _fixture.Diagnostics,
            hoveredNode: null,
            hoveredPod: null,
            LayoutMode.SingleTree);
    }
}
