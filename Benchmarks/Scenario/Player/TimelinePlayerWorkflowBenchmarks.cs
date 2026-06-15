using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Shared.Player;
using ChangeTrace.Player.Enums;

namespace ChangeTrace.Benchmarks.Scenario.Player;

/// <summary>
/// Benchmarks realistic timeline-player workflows composed of public control APIs.
/// </summary>
/// <remarks>
/// Covers mixed seek/play/pause/resume flows, chunked manual playback ticks, loop-boundary
/// handling, and backward stepping from an end-positioned player. These scenarios exercise
/// orchestration overhead above the lower-level cursor, clock, and seek benchmarks.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Player)]
public class TimelinePlayerWorkflowBenchmarks
{
    private PlayerBenchmarkFixture _fixture = null!;
    private double _chunkSeconds;

    /// <summary>
    /// Number of synthetic timeline events used by the workflow scenarios.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Prepares deterministic player state for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _fixture = PlayerBenchmarkFixture.Create(EventCount);
        _chunkSeconds = Math.Max(1, EventCount / 64.0);
    }

    /// <summary>
    /// Performs several public seeks across the timeline and assembles one diagnostics snapshot.
    /// </summary>
    [Benchmark(Baseline = true)]
    public double SeekAcrossTimelineHotspotsAndGetDiagnostics()
    {
        using var harness = _fixture.CreateTimelinePlayerHarness();
        var duration = harness.Player.DurationSeconds;

        harness.Player.SeekRelative(duration * 0.10);
        harness.Player.SeekRelative(duration * 0.35);
        harness.Player.SeekRelative(-duration * 0.20);
        harness.Player.SeekRelative(duration * 0.55);

        var diagnostics = harness.Player.GetDiagnostics();
        return diagnostics.Progress + diagnostics.PositionSeconds;
    }

    /// <summary>
    /// Plays halfway in chunks, pauses, seeks again, resumes, then drains the remainder.
    /// </summary>
    [Benchmark]
    public int PlayPauseResumeDrainInChunks()
    {
        using var harness = _fixture.CreateTimelinePlayerHarness();
        var duration = harness.Player.DurationSeconds;
        var half = duration * 0.5;

        harness.Player.Play();

        DrainToPosition(harness, half);

        harness.Player.Pause();
        harness.Player.SeekRelative(duration * 0.10);
        harness.Player.Play();

        DrainToPosition(harness, duration);

        return harness.EventsObserved;
    }

    /// <summary>
    /// Seeks to the end and walks backward through the whole timeline one event at a time.
    /// </summary>
    [Benchmark]
    public int StepBackwardFromEndToStart()
    {
        using var harness = _fixture.CreateTimelinePlayerHarness();
        harness.Player.SeekRelative(harness.Player.DurationSeconds);

        var moved = 0;
        while (harness.Player.StepBackward().IsSuccess)
            moved++;

        return moved;
    }

    /// <summary>
    /// Drains the full timeline twice while running in loop mode.
    /// </summary>
    [Benchmark]
    public int LoopModeDrainTwoFullCycles()
    {
        using var harness = _fixture.CreateTimelinePlayerHarness(PlaybackMode.Loop);
        var loops = 0;

        harness.Player.OnLoopCompleted += _ => loops++;
        harness.Player.Play();

        DrainToPosition(harness, harness.Player.DurationSeconds);
        DrainToPosition(harness, harness.Player.DurationSeconds);

        return harness.EventsObserved + loops;
    }

    private void DrainToPosition(PlayerBenchmarkFixture.PlayerBenchmarkHarness harness, double targetPosition)
    {
        var duration = harness.Player.DurationSeconds;
        var current = harness.Clock.VirtualNow;

        while (current < targetPosition)
        {
            current = Math.Min(targetPosition, current + _chunkSeconds);
            harness.Clock.SnapPosition(Math.Min(duration, current));
            harness.Transport.TriggerTick();
        }
    }
}
