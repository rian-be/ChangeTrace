using BenchmarkDotNet.Attributes;
using ChangeTrace.Core.Aggregators;
using ChangeTrace.Core.Events.Semantic;

namespace ChangeTrace.Benchmarks.Micro.Core;

/// <summary>
/// Benchmarks file coupling pair generation from commit bundles.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Core, BenchmarkCategories.Aggregation, BenchmarkCategories.FileCoupling)]
public class FileCouplingAggregatorBenchmarks
{
    private CommitBundleEvent[] _bundles = null!;

    [Params(1_000, 10_000)]
    public int CommitCount { get; set; }

    [Params(4, 12, 32)]
    public int FilesPerCommit { get; set; }

    [GlobalSetup]
    public void Setup()
        => _bundles = CreateCommitBundles(CommitCount, FilesPerCommit);

    [Benchmark]
    public int GenerateFileCouplings()
    {
        using var writer = new SemanticEventWriter<FileCouplingEvent>();
        var aggregator = new FileCouplingAggregator(writer);

        foreach (var bundle in _bundles)
            aggregator.Process(bundle);

        aggregator.Flush();

        return writer.Count;
    }

    private static CommitBundleEvent[] CreateCommitBundles(
        int commitCount,
        int filesPerCommit)
    {
        var bundles = new CommitBundleEvent[commitCount];

        for (var i = 0; i < bundles.Length; i++)
        {
            var files = new string[filesPerCommit];

            for (var fileIndex = 0; fileIndex < files.Length; fileIndex++)
                files[fileIndex] = $"src/module-{i % 128}/file-{fileIndex}.cs";

            bundles[i] = new CommitBundleEvent(
                $"commit-{i:x8}",
                $"actor-{i % 64}",
                i,
                files);
        }

        return bundles;
    }
}
