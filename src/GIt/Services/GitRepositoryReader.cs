using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.History.Backends;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Facade for repository history reading and cloning.
/// Backend-specific history extraction lives in dedicated readers.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class GitRepositoryReader : IGitRepositoryReader
{
    private static readonly Lazy<bool> GitCliAvailable = new(IsGitCliAvailableCore);

    private readonly ILogger<GitRepositoryReader> _logger;
    private readonly IReadOnlyDictionary<GitHistoryReaderBackend, ICommitHistoryReaderBackend> _historyReaders;

    public GitRepositoryReader(ILogger<GitRepositoryReader> logger)
    {
        _logger = logger;
        _historyReaders = new ICommitHistoryReaderBackend[]
            {
                new LibGit2SharpHistoryReader(logger),
                new GitCliHistoryReader(logger)
            }
            .ToDictionary(reader => reader.Backend);
    }

    public async Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken = default)
    {
        var streamResult = await ReadCommitsStreamAsync(repositoryPath, options, cancellationToken);
        if (streamResult.IsFailure)
            return Result<IReadOnlyList<CommitData>>.Failure(streamResult.Error!);

        try
        {
            var commits = new List<CommitData>();
            await foreach (var commit in streamResult.Value.WithCancellation(cancellationToken))
            {
                commits.Add(commit);
            }

            var orderedCommits = commits
                .OrderBy(commit => commit.Timestamp.UnixSeconds)
                .ToList()
                .AsReadOnly();

            _logger.LogInformation("Read {Count} commits", orderedCommits.Count);
            return Result<IReadOnlyList<CommitData>>.Success(orderedCommits);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read repository");
            return Result<IReadOnlyList<CommitData>>.Failure("Failed to read repository", ex);
        }
    }

    public Task<Result<IAsyncEnumerable<CommitData>>> ReadCommitsStreamAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!ShouldPreferGitCli(options))
            return TryReadWithBackendAsync(repositoryPath, options, cancellationToken);

        return ReadCommitsStreamCoreAsync(repositoryPath, options, cancellationToken);
    }

    private async Task<Result<IAsyncEnumerable<CommitData>>> ReadCommitsStreamCoreAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken)
    {
        if (ShouldPreferGitCli(options))
        {
            var gitCliOptions = options with { Backend = GitHistoryReaderBackend.GitCli };
            var gitCliResult = await TryReadWithBackendAsync(repositoryPath, gitCliOptions, cancellationToken);
            if (gitCliResult.IsSuccess)
            {
                _logger.LogInformation(
                    "Using Git CLI backend for file-change extraction throughput.");
                return gitCliResult;
            }

            _logger.LogWarning(
                "Git CLI backend failed for file-change extraction, falling back to LibGit2Sharp: {Error}",
                gitCliResult.Error);
        }

        return await TryReadWithBackendAsync(repositoryPath, options, cancellationToken);
    }

    private bool ShouldPreferGitCli(GitReaderOptions options)
        => options.Backend == GitHistoryReaderBackend.LibGit2Sharp
           && options.IncludeFileChanges
           && GitCliAvailable.Value;

    private Task<Result<IAsyncEnumerable<CommitData>>> TryReadWithBackendAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken)
    {
        return !_historyReaders.TryGetValue(options.Backend, out var reader)
            ? Task.FromResult(Result<IAsyncEnumerable<CommitData>>.Failure($"Unsupported Git history backend: {options.Backend}"))
            : reader.ReadCommitsStreamAsync(repositoryPath, options, cancellationToken);
    }

    private static bool IsGitCliAvailableCore()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.StartInfo.ArgumentList.Add("--version");
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Result> CloneAsync(
        string url,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                _logger.LogInformation("Cloning {Url} to {Path}", url, destinationPath);

                if (Directory.Exists(destinationPath))
                    Directory.Delete(destinationPath, true);

                Directory.CreateDirectory(destinationPath);

                var options = new CloneOptions
                {
                    FetchOptions =
                    {
                        OnTransferProgress = progress =>
                        {
                            if (progress.TotalObjects > 0 && progress.ReceivedObjects % 100 == 0)
                            {
                                _logger.LogDebug(
                                    "Clone progress: {Received}/{Total}",
                                    progress.ReceivedObjects,
                                    progress.TotalObjects);
                            }

                            return !cancellationToken.IsCancellationRequested;
                        }
                    }
                };

                Repository.Clone(url, destinationPath, options);

                _logger.LogInformation("Clone complete");
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Clone failed");
                return Result.Failure("Clone failed", ex);
            }
        }, cancellationToken);
    }
}
