using BenchmarkDotNet.Attributes;

namespace ChangeTrace.Benchmarks.Player;

/// <summary>
/// Benchmarks creation of fully wired timeline players.
/// </summary>
/// <remarks>
/// Covers factory wiring, timeline duration calculation, timeline normalization,
/// cursor creation, seekable timeline creation, and transport setup.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class TimelinePlayerFactoryBenchmarks
{
    private PlayerBenchmarkFixture _fixture = null!;

    /// <summary>
    /// Number of synthetic timeline events used to create the player.
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
    /// Creates and disposes a fully wired timeline player.
    /// </summary>
    [Benchmark]
    public double CreateTimelinePlayer()
    {
        using var player = _fixture.CreatePlayerFactory()
            .Create(PlayerBenchmarkFixture.CreateTimeline(EventCount));

        return player.DurationSeconds;
    }
}
