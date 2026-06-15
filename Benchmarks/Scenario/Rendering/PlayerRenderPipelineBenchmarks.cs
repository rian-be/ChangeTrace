using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Rendering;
using ChangeTrace.Benchmarks.Shared.Rendering;

namespace ChangeTrace.Benchmarks.Scenario.Rendering;

/// <summary>
/// Benchmarks CPU-side rendering driven by the real timeline player.
/// </summary>
/// <remarks>
/// Covers the bridge between playback and rendering: player event drain, render-event aggregation,
/// semantic translation, scene command dispatch, frame update, and frame submission.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Player, BenchmarkCategories.Rendering)]
public class PlayerRenderPipelineBenchmarks
{
    private PlayerRenderBenchmarkFixture _fixture = null!;

    /// <summary>
    /// Number of synthetic trace events fed through player and render pipeline.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic player-driven render benchmark state for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
        => _fixture = PlayerRenderBenchmarkFixture.Create(EventCount);

    /// <summary>
    /// Drains all queued playback events in one manual tick, then submits one render frame.
    /// </summary>
    [Benchmark(Baseline = true)]
    public int DrainAllEventsAndRenderSingleFrame()
    {
        using var harness = _fixture.CreateHarness();

        harness.Player.Play();
        harness.Clock.SnapPosition(harness.Player.DurationSeconds);
        harness.Transport.TriggerTick();
        harness.Pipeline.OnProgress(harness.Player.Progress);

        return harness.Output.LastObjectCount;
    }

    /// <summary>
    /// Advances playback and rendering incrementally through the entire synthetic timeline.
    /// </summary>
    [Benchmark]
    public int StepPlaybackAndRenderEveryTick()
    {
        using var harness = _fixture.CreateHarness();

        harness.Player.Play();

        for (var second = 1; second <= EventCount; second++)
        {
            harness.Clock.SnapPosition(second);
            harness.Transport.TriggerTick();
            harness.Pipeline.OnProgress(harness.Player.Progress);
        }

        return harness.Output.SubmittedFrames + harness.Scene.Nodes.Count + harness.Scene.Edges.Count;
    }
}
