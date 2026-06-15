using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Shared.Core;
using ChangeTrace.Core.Services;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Services;
using ChangeTrace.GIt.Services.Sidecars;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.Benchmarks.GIt.Benchmarks;

/// <summary>
/// Benchmarks full .gittrace persistence through <see cref="TimelineRepositoryMsgPack"/>,
/// including serializer and file I/O.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Git, BenchmarkCategories.Serialization)]
public class TimelineRepositoryMsgPackBenchmarks
{
    private ILoggerFactory _loggerFactory = null!;
    private TimelineRepositoryMsgPack _repository = null!;
    private Timeline _timeline = null!;
    private string _directoryPath = null!;
    private string _timelinePath = null!;

    [Params(1_000, 10_000, 100_000)]
    public int CommitCount { get; set; }

    [Params(4, 12)]
    public int FilesPerCommit { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.None));

        var serializer = new MessagePackSerializer<Timeline>(
            [new TimelineMessagePackFormatter()]);
        var fileManager = new FileManager();

        _repository = new TimelineRepositoryMsgPack(
            _loggerFactory.CreateLogger<TimelineRepositoryMsgPack>(),
            serializer,
            fileManager,
            new PullRequestSidecarHandler(_loggerFactory.CreateLogger<PullRequestSidecarHandler>(), fileManager),
            new MergeSidecarHandler(_loggerFactory.CreateLogger<MergeSidecarHandler>(), fileManager));

        _timeline = TimelineBenchmarkFixture.Create(
            CommitCount,
            FilesPerCommit).Timeline;

        _directoryPath = Directory.CreateTempSubdirectory("ChangeTrace.Benchmarks.TimelineRepository.").FullName;
        _timelinePath = Path.Combine(_directoryPath, "timeline");

        var saveResult = await _repository.SaveAsync(_timeline, _timelinePath);
        if (saveResult.IsFailure)
            throw new InvalidOperationException(saveResult.Error);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _loggerFactory.Dispose();

        if (Directory.Exists(_directoryPath))
            Directory.Delete(_directoryPath, recursive: true);
    }

    [Benchmark]
    public async Task<int> SaveTimeline()
    {
        var result = await _repository.SaveAsync(_timeline, _timelinePath);
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);

        return new FileInfo(_timelinePath + ".gittrace").Length > int.MaxValue
            ? int.MaxValue
            : (int)new FileInfo(_timelinePath + ".gittrace").Length;
    }

    [Benchmark]
    public async Task<int> LoadTimeline()
    {
        var result = await _repository.LoadAsync(_timelinePath + ".gittrace");
        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);

        return result.Value.Count;
    }
}
