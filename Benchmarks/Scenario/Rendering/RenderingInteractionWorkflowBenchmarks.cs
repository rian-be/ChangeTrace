using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Shared.Rendering;
using ChangeTrace.Rendering;

namespace ChangeTrace.Benchmarks.Scenario.Rendering;

/// <summary>
/// Benchmarks interactive rendering workflows on an already populated scene.
/// </summary>
/// <remarks>
/// Covers repeated hover picking, camera pan/zoom interaction, layout toggling,
/// and frame submission through the real rendering pipeline after the scene has
/// already been built from timeline playback events.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Rendering)]
public class RenderingInteractionWorkflowBenchmarks
{
    private static readonly Vec2[] MouseSweepPositions =
    [
        new(120, 120),
        new(320, 180),
        new(640, 240),
        new(960, 320),
        new(1280, 400),
        new(1600, 520),
        new(1800, 760),
        new(1440, 900),
        new(1080, 820),
        new(720, 680),
        new(420, 540),
        new(180, 320)
    ];

    private PlayerRenderBenchmarkFixture _fixture = null!;
    private PlayerRenderBenchmarkFixture.PlayerRenderBenchmarkHarness _harness = null!;

    /// <summary>
    /// Number of synthetic trace events used to populate the rendering scene.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic player-driven rendering input for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
        => _fixture = PlayerRenderBenchmarkFixture.Create(EventCount);

    /// <summary>
    /// Rebuilds a populated scene before each measured interaction workflow.
    /// </summary>
    [IterationSetup]
    public void IterationSetup()
    {
        _harness = _fixture.CreateHarness();
        PrimePopulatedScene(_harness);
    }

    /// <summary>
    /// Releases pipeline resources after each measured interaction workflow.
    /// </summary>
    [IterationCleanup]
    public void IterationCleanup()
        => _harness.Dispose();

    /// <summary>
    /// Sweeps the mouse across the viewport and submits a frame after each hover update.
    /// </summary>
    [Benchmark(Baseline = true)]
    public int SweepHoverAndRenderFrames()
    {
        foreach (var mouse in MouseSweepPositions)
        {
            _harness.Pipeline.UpdateMouse(mouse);
            _harness.Pipeline.OnProgress(_harness.Player.Progress);
        }

        return _harness.Output.SubmittedFrames + _harness.Output.LastObjectCount;
    }

    /// <summary>
    /// Applies repeated pan and zoom interactions while rendering incremental frames.
    /// </summary>
    [Benchmark]
    public int PanZoomAndRenderFrames()
    {
        for (var i = 0; i < MouseSweepPositions.Length; i++)
        {
            _harness.Pipeline.PanCamera(new Vec2(6 + i, -3 + i * 0.5f));
            _harness.Pipeline.ZoomCamera((i % 2 == 0 ? 0.035f : -0.02f));
            _harness.Pipeline.OnProgress(_harness.Player.Progress);
        }

        return _harness.Output.SubmittedFrames + _harness.Output.LastObjectCount;
    }

    /// <summary>
    /// Alternates layout mode while rendering frames and refreshing hover state.
    /// </summary>
    [Benchmark]
    public int ToggleLayoutAndRenderFrames()
    {
        foreach (var mouse in MouseSweepPositions)
        {
            _harness.Pipeline.ToggleLayoutMode();
            _harness.Pipeline.UpdateMouse(mouse);
            _harness.Pipeline.OnProgress(_harness.Player.Progress);
        }

        return _harness.Output.SubmittedFrames + _harness.Output.LastObjectCount;
    }

    private static void PrimePopulatedScene(PlayerRenderBenchmarkFixture.PlayerRenderBenchmarkHarness harness)
    {
        harness.Player.Play();
        harness.Clock.SnapPosition(harness.Player.DurationSeconds);
        harness.Transport.TriggerTick();
        harness.Pipeline.OnProgress(harness.Player.Progress);
    }
}
