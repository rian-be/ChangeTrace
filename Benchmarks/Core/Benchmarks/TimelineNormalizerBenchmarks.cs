using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Core.Fixtures;
using ChangeTrace.Core.Timelines;

namespace ChangeTrace.Benchmarks.Core.Benchmarks;

/// <summary>
/// Benchmarks timeline sorting and playback time normalization.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Core, BenchmarkCategories.Timeline)]
public class TimelineNormalizerBenchmarks
{
    private Timeline _sortedTimeline = null!;
    private Timeline _reversedTimeline = null!;
    private Timeline _sortedIterationTimeline = null!;
    private Timeline _reversedIterationTimeline = null!;

    [Params(1_000, 10_000, 100_000)]
    public int CommitCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var commits = TimelineBenchmarkFixture.CreateCommits(
            CommitCount,
            filesPerCommit: 4,
            includeMerges: true);

        _sortedTimeline = TimelineBenchmarkFixture.CreateTimeline(commits);
        _reversedTimeline = new Timeline(TimelineBenchmarkFixture.RepositoryId);
        _reversedTimeline.AddEvents(_sortedTimeline.Events.Reverse());
    }

    [Benchmark(Baseline = true)]
    public int NormalizeAlreadySorted()
    {
        var timeline = TimelineBenchmarkFixture.CloneTimeline(_sortedTimeline);
        TimelineNormalizer.Normalize(timeline);
        return timeline.Count;
    }

    [Benchmark]
    public int NormalizeReversed()
    {
        var timeline = TimelineBenchmarkFixture.CloneTimeline(_reversedTimeline);
        TimelineNormalizer.Normalize(timeline);
        return timeline.Count;
    }

    [IterationSetup(Target = nameof(NormalizeAlreadySortedInPlace))]
    public void SetupSortedIteration()
        => _sortedIterationTimeline = TimelineBenchmarkFixture.CloneTimeline(_sortedTimeline);

    [Benchmark]
    public int NormalizeAlreadySortedInPlace()
    {
        TimelineNormalizer.Normalize(_sortedIterationTimeline);
        return _sortedIterationTimeline.Count;
    }

    [IterationSetup(Target = nameof(NormalizeReversedInPlace))]
    public void SetupReversedIteration()
        => _reversedIterationTimeline = TimelineBenchmarkFixture.CloneTimeline(_reversedTimeline);

    [Benchmark]
    public int NormalizeReversedInPlace()
    {
        TimelineNormalizer.Normalize(_reversedIterationTimeline);
        return _reversedIterationTimeline.Count;
    }
}
