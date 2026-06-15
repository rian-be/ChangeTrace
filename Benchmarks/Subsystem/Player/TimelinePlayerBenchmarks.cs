using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Shared.Player;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Benchmarks.Subsystem.Player;

/// <summary>
/// Benchmarks high-level player orchestration on top of cursor, seek, and transport components.
/// </summary>
/// <remarks>
/// Covers the coordinator layer directly: delegated seek, event stepping through the public player API,
/// single-tick event draining, and diagnostics snapshot assembly after playback state changes.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Player)]
public class TimelinePlayerBenchmarks
{
    private PlayerBenchmarkFixture _fixture = null!;
    private Timestamp _middle = default;

    /// <summary>
    /// Number of synthetic timeline events used by the player.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic player benchmark state for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        _fixture = PlayerBenchmarkFixture.Create(EventCount);
        _middle = Timestamp.Create(EventCount / 2).Value;
    }

    /// <summary>
    /// Seeks through the public player API, then assembles a diagnostics snapshot.
    /// </summary>
    [Benchmark(Baseline = true)]
    public double SeekToMiddleAndGetDiagnostics()
    {
        using var harness = _fixture.CreateTimelinePlayerHarness();
        harness.Player.Seek(_middle);
        return harness.Player.GetDiagnostics().Progress;
    }

    /// <summary>
    /// Steps forward through all events by using the public player API.
    /// </summary>
    [Benchmark]
    public int StepForwardThroughAllEvents()
    {
        using var harness = _fixture.CreateTimelinePlayerHarness();

        while (harness.Player.StepForward().IsSuccess)
        {
        }

        return harness.EventsObserved;
    }

    /// <summary>
    /// Drains all events by advancing virtual time and triggering a single manual transport tick.
    /// </summary>
    [Benchmark]
    public int DrainForwardAllEventsInSingleTick()
    {
        using var harness = _fixture.CreateTimelinePlayerHarness();
        harness.Player.Play();
        harness.Clock.SnapPosition(harness.Player.DurationSeconds);
        harness.Transport.TriggerTick();
        return harness.EventsObserved;
    }
}
