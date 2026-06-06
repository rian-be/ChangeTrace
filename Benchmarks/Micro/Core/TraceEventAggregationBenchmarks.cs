using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Core.Fixtures;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Processors;

namespace ChangeTrace.Benchmarks.Core.Benchmarks;

/// <summary>
/// Benchmarks semantic aggregation from raw trace events.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Core, BenchmarkCategories.Aggregation)]
public class TraceEventAggregationBenchmarks
{
    private TraceEvent[] _events = null!;

    [Params(1_000, 10_000, 100_000)]
    public int CommitCount { get; set; }

    [GlobalSetup]
    public void Setup()
        => _events = TimelineBenchmarkFixture.Create(
            CommitCount,
            filesPerCommit: 4,
            includeMerges: true).TraceEvents;

    [Benchmark(Baseline = true)]
    public int AggregateCommitBundles()
    {
        using var stage = new TraceEventAggregationStage(RenderEventKinds.Commit);

        foreach (var evt in _events)
            stage.Process(evt);

        stage.Flush();

        return stage.GetWriter<CommitBundleEvent>().Count;
    }

    [Benchmark]
    public int AggregatePrimarySemanticEvents()
    {
        using var stage = new TraceEventAggregationStage(
            RenderEventKinds.Commit |
            RenderEventKinds.Branch |
            RenderEventKinds.Merge);

        foreach (var evt in _events)
            stage.Process(evt);

        stage.Flush();

        return
            stage.GetWriter<CommitBundleEvent>().Count +
            stage.GetWriter<BranchEvent>().Count +
            stage.GetWriter<MergeEvent>().Count;
    }
}
