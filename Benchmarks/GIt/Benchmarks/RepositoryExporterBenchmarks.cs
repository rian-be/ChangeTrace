using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.Core.Fixtures;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Services;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChangeTrace.Benchmarks.GIt.Benchmarks;

/// <summary>
/// Benchmarks export orchestration without clone, network, or file persistence.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Git, BenchmarkCategories.Export)]
public class RepositoryExporterBenchmarks
{
    private static readonly ExportOptions ExportOptions = new()
    {
        IncludeFileChanges = true,
        IncludeBranchEvents = true,
        IncludeMergeDetection = true,
        EnrichWithPullRequests = false
    };

    private RepositoryExporter _exporter = null!;
    private FakeGitRepositoryReader _reader = null!;
    private string _repositoryPath = null!;

    [Params(1_000, 10_000, 100_000)]
    public int CommitCount { get; set; }

    [Params(4)]
    public int FilesPerCommit { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _repositoryPath = Directory.CreateTempSubdirectory("ChangeTrace.Benchmarks.Export.").FullName;
        _reader = new FakeGitRepositoryReader(
            TimelineBenchmarkFixture.CreateCommits(
                CommitCount,
                FilesPerCommit,
                includeMerges: true));

        _exporter = new RepositoryExporter(
            _reader,
            new TimelineBuilder(NullLogger<TimelineBuilder>.Instance),
            new NoopTimelineRepository(),
            NullLogger<RepositoryExporter>.Instance);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_repositoryPath))
            Directory.Delete(_repositoryPath, recursive: true);
    }

    [Benchmark]
    public async Task<int> ExportLocalRepository()
    {
        var result = await _exporter.ExportAsync(
            _repositoryPath,
            ExportOptions);

        return result.Value.Count;
    }

    private sealed class FakeGitRepositoryReader(
        IReadOnlyList<CommitData> commits) : IGitRepositoryReader
    {
        public Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
            string repositoryPath,
            GitReaderOptions options,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<CommitData> selected = options.MaxCommits > 0
                ? commits.Take(options.MaxCommits).ToArray()
                : commits;

            return Task.FromResult(Result<IReadOnlyList<CommitData>>.Success(selected));
        }

        public Task<Result<IAsyncEnumerable<CommitData>>> ReadCommitsStreamAsync(
            string repositoryPath,
            GitReaderOptions options,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<CommitData> selected = options.MaxCommits > 0
                ? commits.Take(options.MaxCommits)
                : commits;

            return Task.FromResult(Result<IAsyncEnumerable<CommitData>>.Success(
                StreamCommits(selected, cancellationToken)));
        }

        private static async IAsyncEnumerable<CommitData> StreamCommits(
            IEnumerable<CommitData> commits,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var commit in commits)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return commit;
            }
        }

        public Task<Result> CloneAsync(
            string url,
            string destinationPath,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());
    }

    private sealed class NoopTimelineRepository : ITimelineRepository
    {
        public Task<Result> SaveAsync(
            Timeline timeline,
            string filePath,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        public Task<Result<Timeline>> LoadAsync(
            string filePath,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Timeline>.Failure("Not used by exporter benchmarks."));
    }
}
