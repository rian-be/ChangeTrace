using LibGit2Sharp;

namespace ChangeTrace.Benchmarks.GIt.Fixtures;

/// <summary>
/// Creates a local Git repository fixture for LibGit2Sharp reader benchmarks.
/// </summary>
internal sealed class GitRepositoryBenchmarkFixture : IDisposable
{
    private GitRepositoryBenchmarkFixture(string repositoryPath)
        => RepositoryPath = repositoryPath;

    public string RepositoryPath { get; }

    public static GitRepositoryBenchmarkFixture Create(
        int commitCount,
        int filesPerCommit)
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "ChangeTrace.Benchmarks",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);
        Repository.Init(root);

        using var repo = new Repository(root);
        var author = new Signature(
            "Benchmark User",
            "benchmark@example.com",
            new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

        for (var commitIndex = 0; commitIndex < commitCount; commitIndex++)
        {
            for (var fileIndex = 0; fileIndex < filesPerCommit; fileIndex++)
            {
                var directory = Path.Combine(root, "src", $"module-{commitIndex % 32}");
                Directory.CreateDirectory(directory);

                var path = Path.Combine(directory, $"file-{fileIndex}.txt");
                File.AppendAllText(
                    path,
                    $"commit={commitIndex}; file={fileIndex}{Environment.NewLine}");
            }

            Commands.Stage(repo, "*");
            repo.Commit($"Benchmark commit {commitIndex}", author, author);
        }

        return new GitRepositoryBenchmarkFixture(root);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(RepositoryPath))
                Directory.Delete(RepositoryPath, recursive: true);
        }
        catch
        {
            // Best effort cleanup for benchmark temp repositories.
        }
    }
}
