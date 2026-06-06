using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Core.Fixtures;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChangeTrace.Benchmarks.Core.Benchmarks;

/// <summary>
/// Benchmarks timeline construction from already-read commit data.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Core, BenchmarkCategories.Timeline)]
public class TimelineBuilderBenchmarks
{
    private static readonly TimelineBuilderOptions CommitsOnlyOptions = new(
        IncludeFileChanges: false,
        IncludeBranchEvents: false,
        IncludeMergeDetection: false,
        RepositoryId: TimelineBenchmarkFixture.RepositoryId);

    private static readonly TimelineBuilderOptions FileChangesOptions = new(
        IncludeFileChanges: true,
        IncludeBranchEvents: false,
        IncludeMergeDetection: false,
        RepositoryId: TimelineBenchmarkFixture.RepositoryId);

    private static readonly TimelineBuilderOptions AllEventsOptions = new(
        IncludeFileChanges: true,
        IncludeBranchEvents: true,
        IncludeMergeDetection: true,
        RepositoryId: TimelineBenchmarkFixture.RepositoryId);

    private TimelineBuilder _builder = null!;
    private IReadOnlyList<CommitData> _commits = null!;

    [Params(1_000, 10_000, 100_000)]
    public int CommitCount { get; set; }

    [Params(1, 4, 12)]
    public int FilesPerCommit { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _builder = new TimelineBuilder(NullLogger<TimelineBuilder>.Instance);
        _commits = TimelineBenchmarkFixture.CreateCommits(
            CommitCount,
            FilesPerCommit,
            includeMerges: true);
    }

    [Benchmark(Baseline = true)]
    public int BuildWithCommitsOnly()
        => _builder.Build(_commits, CommitsOnlyOptions).Value.Count;

    [Benchmark]
    public int BuildWithFileChanges()
        => _builder.Build(_commits, FileChangesOptions).Value.Count;

    [Benchmark]
    public int BuildWithAllEvents()
        => _builder.Build(_commits, AllEventsOptions).Value.Count;
}
