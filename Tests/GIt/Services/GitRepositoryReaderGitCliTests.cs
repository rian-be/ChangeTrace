using System.Diagnostics;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ChangeTrace.Tests.GIt.Services;

public sealed class GitRepositoryReaderGitCliTests : IDisposable
{
    private readonly string _repositoryPath = Path.Combine(
        Path.GetTempPath(),
        "ChangeTrace.Tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task ReadCommitsAsync_GitCli_MatchesLibGit2SharpHistoryShape()
    {
        CreateRepository();

        var reader = new GitRepositoryReader(NullLogger<GitRepositoryReader>.Instance);

        var libGitResult = await reader.ReadCommitsAsync(
            _repositoryPath,
            new GitReaderOptions(IncludeFileChanges: true));

        var gitCliResult = await reader.ReadCommitsAsync(
            _repositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: true,
                Backend: GitHistoryReaderBackend.GitCli));

        Assert.True(libGitResult.IsSuccess, libGitResult.Error);
        Assert.True(gitCliResult.IsSuccess, gitCliResult.Error);

        var libGitCommits = libGitResult.Value;
        var gitCliCommits = gitCliResult.Value;

        Assert.Equal(libGitCommits.Count, gitCliCommits.Count);

        for (var index = 0; index < libGitCommits.Count; index++)
        {
            var expected = libGitCommits[index];
            var actual = gitCliCommits[index];

            Assert.Equal(expected.Sha.Value, actual.Sha.Value);
            Assert.Equal(expected.Author.Value, actual.Author.Value);
            Assert.Equal(expected.Timestamp.UnixSeconds, actual.Timestamp.UnixSeconds);
            Assert.Equal(expected.Message, actual.Message);
            Assert.Equal(expected.IsMerge, actual.IsMerge);
            Assert.Equal(
                expected.ParentShas.Select(parent => parent.Value),
                actual.ParentShas.Select(parent => parent.Value));
            AssertFileChanges(expected.FileChanges, actual.FileChanges);
        }
    }

    [Fact]
    public async Task ReadCommitsAsync_GitCli_HonorsMaxCommitsAndDateRange()
    {
        CreateRepository();

        var reader = new GitRepositoryReader(NullLogger<GitRepositoryReader>.Instance);
        var result = await reader.ReadCommitsAsync(
            _repositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: false,
                MaxCommits: 2,
                StartDate: new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
                EndDate: new DateTimeOffset(2026, 1, 3, 23, 59, 59, TimeSpan.Zero),
                Backend: GitHistoryReaderBackend.GitCli));

        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, commit => Assert.Empty(commit.FileChanges));
        Assert.Equal(["Modify alpha", "Rename alpha"], result.Value.Select(commit => commit.Message));
    }

    [Fact]
    public async Task ReadCommitsAsync_GitCli_CanSkipRenameDetection()
    {
        CreateRepository();

        var reader = new GitRepositoryReader(NullLogger<GitRepositoryReader>.Instance);
        var result = await reader.ReadCommitsAsync(
            _repositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: true,
                Backend: GitHistoryReaderBackend.GitCli,
                DetectRenames: false));

        Assert.True(result.IsSuccess, result.Error);

        var renameCommit = Assert.Single(result.Value, commit => commit.Message == "Rename alpha");
        Assert.Contains(renameCommit.FileChanges, change =>
            change.Kind == FileChangeKind.Deleted && change.Path.Value == "alpha.txt");
        Assert.Contains(renameCommit.FileChanges, change =>
            change.Kind == FileChangeKind.Added && change.Path.Value == "src-alpha.txt");
    }

    [Fact]
    public async Task ReadCommitsAsync_GitCli_HandlesQuotedPathNames()
    {
        CreateRepository();

        var reader = new GitRepositoryReader(NullLogger<GitRepositoryReader>.Instance);
        var result = await reader.ReadCommitsAsync(
            _repositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: true,
                Backend: GitHistoryReaderBackend.GitCli));

        Assert.True(result.IsSuccess, result.Error);

        var commit = Assert.Single(result.Value, commit => commit.Message == "Add quoted path");
        var change = Assert.Single(commit.FileChanges);
        Assert.Equal(FileChangeKind.Added, change.Kind);
        Assert.Equal($"docs/name\twith{'\n'}line.txt", change.Path.Value);
    }

    [Fact]
    public async Task ReadCommitsAsync_DefaultBackendWithFileChanges_MatchesGitCliForQuotedPathNames()
    {
        CreateRepository();

        var reader = new GitRepositoryReader(NullLogger<GitRepositoryReader>.Instance);

        var defaultResult = await reader.ReadCommitsAsync(
            _repositoryPath,
            new GitReaderOptions(IncludeFileChanges: true));

        var gitCliResult = await reader.ReadCommitsAsync(
            _repositoryPath,
            new GitReaderOptions(
                IncludeFileChanges: true,
                Backend: GitHistoryReaderBackend.GitCli));

        Assert.True(defaultResult.IsSuccess, defaultResult.Error);
        Assert.True(gitCliResult.IsSuccess, gitCliResult.Error);

        var defaultCommit = Assert.Single(defaultResult.Value, commit => commit.Message == "Add quoted path");
        var gitCliCommit = Assert.Single(gitCliResult.Value, commit => commit.Message == "Add quoted path");
        var defaultChange = Assert.Single(defaultCommit.FileChanges);
        var gitCliChange = Assert.Single(gitCliCommit.FileChanges);

        Assert.Equal(gitCliChange.Path.Value, defaultChange.Path.Value);
        Assert.Equal(gitCliChange.Kind, defaultChange.Kind);
        Assert.Equal(gitCliChange.OldPath?.Value, defaultChange.OldPath?.Value);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_repositoryPath))
                Directory.Delete(_repositoryPath, recursive: true);
        }
        catch
        {
            // Best effort cleanup for temporary Git repositories.
        }
    }

    private static void AssertFileChanges(
        IReadOnlyList<ChangeTrace.Core.Enums.FileChange> expected,
        IReadOnlyList<ChangeTrace.Core.Enums.FileChange> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        for (var index = 0; index < expected.Count; index++)
        {
            Assert.Equal(expected[index].Path.Value, actual[index].Path.Value);
            Assert.Equal(expected[index].Kind, actual[index].Kind);
            Assert.Equal(expected[index].OldPath?.Value, actual[index].OldPath?.Value);
        }
    }

    private void CreateRepository()
    {
        Directory.CreateDirectory(_repositoryPath);
        RunGit("init");
        RunGit("config", "user.name", "ChangeTrace Test");
        RunGit("config", "user.email", "changetrace@example.com");

        File.WriteAllText(Path.Combine(_repositoryPath, "alpha.txt"), "one");
        Commit("Initial commit", "2026-01-01T00:00:00Z", "add", ".");

        File.AppendAllText(Path.Combine(_repositoryPath, "alpha.txt"), $"{Environment.NewLine}two");
        File.WriteAllText(Path.Combine(_repositoryPath, "beta.txt"), "beta");
        Commit("Modify alpha", "2026-01-02T00:00:00Z", "add", ".");

        RunGit("mv", "alpha.txt", "src-alpha.txt");
        Commit("Rename alpha", "2026-01-03T00:00:00Z", "add", ".");

        Directory.CreateDirectory(Path.Combine(_repositoryPath, "docs"));
        File.WriteAllText(Path.Combine(_repositoryPath, "docs", $"name\twith{'\n'}line.txt"), "quoted");
        Commit("Add quoted path", "2026-01-04T00:00:00Z", "add", ".");
    }

    private void Commit(string message, string timestamp, params string[] stageArguments)
    {
        RunGit(stageArguments);
        RunGit(
            new Dictionary<string, string>
            {
                ["GIT_AUTHOR_DATE"] = timestamp,
                ["GIT_COMMITTER_DATE"] = timestamp
            },
            "commit",
            "-m",
            message);
    }

    private void RunGit(params string[] arguments)
        => RunGit(null, arguments);

    private void RunGit(IReadOnlyDictionary<string, string>? environment, params string[] arguments)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = _repositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var pair in environment ?? new Dictionary<string, string>())
            process.StartInfo.Environment[pair.Key] = pair.Value;

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed: {output}{error}");
    }
}
