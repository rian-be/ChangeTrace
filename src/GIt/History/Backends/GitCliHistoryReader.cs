using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.History.GitCli;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.History.Backends;

/// <summary>
/// Git CLI based commit history reader.
/// Validates repository access and streams parsed git log output.
/// </summary>
internal sealed class GitCliHistoryReader(ILogger logger) : ICommitHistoryReaderBackend
{
    /// <summary>
    /// Backend type used by this reader.
    /// </summary>
    public GitHistoryReaderBackend Backend => GitHistoryReaderBackend.GitCli;

    /// <summary>
    /// Reads commit history from a Git repository.
    /// </summary>
    public async Task<Result<IAsyncEnumerable<CommitData>>> ReadCommitsStreamAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var validation = await GitCliProcessRunner.RunAsync(
                repositoryPath,
                ["rev-parse", "--git-dir"],
                cancellationToken);

            if (validation.ExitCode != 0)
                return Result<IAsyncEnumerable<CommitData>>.Failure("Invalid Git repository");

            var branches = options.IncludeBranches
                ? await GetCurrentBranchAsync(repositoryPath, cancellationToken)
                : [];
            var arguments = BuildGitLogArguments(options);

            return Result<IAsyncEnumerable<CommitData>>.Success(
                GitCliLogParser.ReadAsync(
                    repositoryPath,
                    arguments,
                    options.IncludeFileChanges,
                    branches,
                    cancellationToken));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to read repository with Git CLI");
            return Result<IAsyncEnumerable<CommitData>>.Failure("Failed to read repository with Git CLI", ex);
        }
    }

    /// <summary>
    /// Builds git log arguments from reader options.
    /// </summary>
    private static List<string> BuildGitLogArguments(GitReaderOptions options)
    {
        var arguments = new List<string>
        {
            "log",
            "--topo-order",
            "--reverse",
            "--date-order",
            "--diff-merges=first-parent",
            $"--pretty=format:{GitCliLogParser.RecordSeparator}%H{GitCliLogParser.UnitSeparator}%an{GitCliLogParser.UnitSeparator}%at{GitCliLogParser.UnitSeparator}%P{GitCliLogParser.UnitSeparator}%s"
        };

        if (options.IncludeFileChanges)
        {
            arguments.Add("--name-status");
            arguments.Add("-z");
            arguments.Add(options.DetectRenames ? "-M" : "--no-renames");
        }

        if (options.MaxCommits > 0)
            arguments.Add($"--max-count={options.MaxCommits}");

        if (options.StartDate.HasValue)
            arguments.Add($"--since=@{options.StartDate.Value.ToUnixTimeSeconds()}");

        if (options.EndDate.HasValue)
            arguments.Add($"--until=@{options.EndDate.Value.ToUnixTimeSeconds()}");

        return arguments;
    }

    /// <summary>
    /// Reads the currently checked out branch.
    /// </summary>
    private static async Task<IReadOnlyList<BranchName>> GetCurrentBranchAsync(
        string repositoryPath,
        CancellationToken cancellationToken)
    {
        var result = await GitCliProcessRunner.RunAsync(
            repositoryPath,
            ["branch", "--show-current"],
            cancellationToken);

        if (result.ExitCode != 0)
            return [];

        var branchName = result.Output.Trim();
        if (string.IsNullOrWhiteSpace(branchName))
            return [];

        var branchResult = BranchName.Create(branchName);
        return branchResult.IsSuccess ? [branchResult.Value] : [];
    }
}
