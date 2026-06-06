using BenchmarkDotNet.Attributes;
using ChangeTrace.Core.Models;
using ChangeTrace.Player.Playback;

namespace ChangeTrace.Benchmarks.Player;

/// <summary>
/// Benchmarks player seek operations.
/// </summary>
/// <remarks>
/// Measures absolute and relative seek paths, including virtual clock snapping,
/// cursor repositioning, and progress calculation.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class SeekableTimelineBenchmarks
{
    private PlayerBenchmarkFixture _fixture = null!;
    private SeekableTimeline _seekable = null!;
    private Timestamp _middle = default;

    /// <summary>
    /// Number of synthetic timeline events used by seek benchmarks.
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
        _seekable = _fixture.CreateSeekable();
        _middle = Timestamp.Create(EventCount / 2).Value;
    }

    /// <summary>
    /// Seeks to the middle of the timeline using an absolute timestamp.
    /// </summary>
    [Benchmark]
    public double SeekToMiddle()
    {
        _seekable.Seek(_middle);
        return _seekable.Progress;
    }

    /// <summary>
    /// Seeks relative to the current position and returns progress.
    /// </summary>
    [Benchmark]
    public double SeekRelativeSmallDelta()
    {
        _seekable.SeekRelative(17);
        return _seekable.Progress;
    }
}
