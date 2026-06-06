using BenchmarkDotNet.Attributes;
using ChangeTrace.Player.Playback;

namespace ChangeTrace.Benchmarks.Player;

/// <summary>
/// Benchmarks low-level timeline cursor navigation.
/// </summary>
/// <remarks>
/// Measures binary seeking and forward draining over synthetic timeline events.
/// These operations are used by player seeking and playback tick event emission.
/// </remarks>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
public class EventCursorBenchmarks
{
    private PlayerBenchmarkFixture _fixture = null!;
    private EventCursor _cursor = null!;

    /// <summary>
    /// Number of synthetic timeline events used by the cursor.
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
        _cursor = _fixture.CreateCursor();
    }

    /// <summary>
    /// Seeks to the middle of the event stream using cursor binary search.
    /// </summary>
    [Benchmark]
    public int SeekToMiddle()
    {
        _cursor.SeekTo(EventCount / 2.0);
        return _cursor.Index;
    }

    /// <summary>
    /// Drains all events forward from a fresh cursor.
    /// </summary>
    [Benchmark]
    public int DrainForwardAll()
    {
        _cursor.ResetToStart();
        return _cursor.DrainForward(EventCount).Count;
    }
}
