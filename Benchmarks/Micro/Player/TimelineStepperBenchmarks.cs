using BenchmarkDotNet.Attributes;

namespace ChangeTrace.Benchmarks.Player;

/// <summary>
/// Benchmarks single-event stepping through a timeline.
/// </summary>
/// <remarks>
/// Measures the cost of repeatedly calling the player stepper until all events have
/// been emitted. This covers cursor movement and virtual clock position snapping.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class TimelineStepperBenchmarks
{
    private PlayerBenchmarkFixture _fixture = null!;

    /// <summary>
    /// Number of synthetic timeline events stepped through by the benchmark.
    /// </summary>
    [Params(1_000, 10_000, 100_000)]
    public int EventCount { get; set; }

    /// <summary>
    /// Creates deterministic player benchmark state for the current event count.
    /// </summary>
    [GlobalSetup]
    public void Setup()
        => _fixture = PlayerBenchmarkFixture.Create(EventCount);

    /// <summary>
    /// Steps forward through all events in a fresh stepper.
    /// </summary>
    [Benchmark]
    public int StepForwardThroughAllEvents()
    {
        var stepper = _fixture.CreateStepper();
        var moved = 0;

        while (stepper.StepForward().IsSuccess)
            moved++;

        return moved;
    }
}
