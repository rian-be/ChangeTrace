using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Core.Fixtures;
using ChangeTrace.Core.Services;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Services;

namespace ChangeTrace.Benchmarks.Core.Benchmarks;

/// <summary>
/// Benchmarks MessagePack serialization for portable .gittrace timeline payloads.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Core, BenchmarkCategories.Serialization)]
public class TimelineSerializationBenchmarks
{
    private MessagePackSerializer<Timeline> _serializer = null!;
    private Timeline _timeline = null!;
    private byte[] _serialized = null!;

    [Params(1_000, 10_000, 100_000)]
    public int CommitCount { get; set; }

    [Params(4)]
    public int FilesPerCommit { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _serializer = new MessagePackSerializer<Timeline>(
            [new TimelineMessagePackFormatter()]);
        _timeline = TimelineBenchmarkFixture.Create(
            CommitCount,
            FilesPerCommit).Timeline;
        _serialized = await _serializer.SerializeAsync(_timeline);
    }

    [Benchmark]
    public async Task<int> SerializeTimeline()
        => (await _serializer.SerializeAsync(_timeline)).Length;

    [Benchmark]
    public async Task<int> DeserializeTimeline()
        => (await _serializer.DeserializeAsync(_serialized)).Count;
}
