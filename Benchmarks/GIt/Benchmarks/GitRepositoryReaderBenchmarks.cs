using BenchmarkDotNet.Attributes;
using ChangeTrace.Benchmarks.GIt.Fixtures;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChangeTrace.Benchmarks.GIt.Benchmarks;

/// <summary>
/// Benchmarks LibGit2Sharp commit reading against a local synthetic repository.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[MinIterationTime(250)]
[BenchmarkCategory(BenchmarkCategories.Git)]
public class GitRepositoryReaderBenchmarks
{
    private GitRepositoryBenchmarkFixture _fixture = null!;
    private GitRepositoryReader _reader = null!;

    [Params(100, 1_000)]
    public int CommitCount { get; set; }

    [Params(2, 8)]
    public int FilesPerCommit { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _fixture = GitRepositoryBenchmarkFixture.Create(
            CommitCount,
            FilesPerCommit);
        _reader = new GitRepositoryReader(
            NullLogger<GitRepositoryReader>.Instance);
    }

    [GlobalCleanup]
    public void Cleanup()
        => _fixture.Dispose();

    [Benchmark(Baseline = true)]
    public async Task<int> ReadCommitsOnly()
    {
        var result = await _reader.ReadCommitsAsync(
            _fixture.RepositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: false,
                MaxCommits: CommitCount));

        return result.Value.Count;
    }

    [Benchmark]
    public async Task<int> ReadCommitsWithFileChanges()
    {
        var result = await _reader.ReadCommitsAsync(
            _fixture.RepositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: true,
                MaxCommits: CommitCount));

        return result.Value.Sum(commit => commit.FileChanges.Count);
    }

    [Benchmark]
    public async Task<int> ReadCommitsOnlyWithGitCli()
    {
        var result = await _reader.ReadCommitsAsync(
            _fixture.RepositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: false,
                MaxCommits: CommitCount,
                Backend: GitHistoryReaderBackend.GitCli));

        return result.Value.Count;
    }

    [Benchmark]
    public async Task<int> ReadCommitsWithFileChangesWithGitCli()
    {
        var result = await _reader.ReadCommitsAsync(
            _fixture.RepositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: true,
                MaxCommits: CommitCount,
                Backend: GitHistoryReaderBackend.GitCli));

        return result.Value.Sum(commit => commit.FileChanges.Count);
    }

    [Benchmark]
    public async Task<int> ReadCommitsWithFileChangesWithGitCliNoRenames()
    {
        var result = await _reader.ReadCommitsAsync(
            _fixture.RepositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: true,
                MaxCommits: CommitCount,
                Backend: GitHistoryReaderBackend.GitCli,
                DetectRenames: false));

        return result.Value.Sum(commit => commit.FileChanges.Count);
    }
}
