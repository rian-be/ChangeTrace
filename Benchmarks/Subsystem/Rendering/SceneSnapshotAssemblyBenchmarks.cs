using BenchmarkDotNet.Attributes;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Benchmarks.Rendering;

/// <summary>
/// Benchmarks immutable scene snapshot assembly.
/// </summary>
/// <remarks>
/// Measures node, avatar, edge visibility, and particle snapshot generation without HUD,
/// camera, frame submission, or OpenGL upload work.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class SceneSnapshotAssemblyBenchmarks
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
    /// Assembles immutable scene snapshots for nodes, avatars, edges, and particles.
    /// </summary>
    [Benchmark]
    public int AssembleSceneSnapshot()
    {
        SceneSnapshot snapshot = _fixture.AssembleSceneSnapshot();
        return snapshot.TotalObjects;
    }
}
