using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.History.Backends;

/// <summary>
/// LibGit2Sharp based commit history reader.
/// Loads commit metadata, branches and file changes.
/// </summary>
internal sealed class LibGit2SharpHistoryReader(ILogger logger)
    : ICommitHistoryReaderBackend
{
    /// <summary>
    /// Backend type used by this reader.
    /// </summary>
    public GitHistoryReaderBackend Backend => GitHistoryReaderBackend.LibGit2Sharp;

    /// <summary>
    /// Reads commit history from a Git repository.
    /// </summary>
    public async Task<Result<IAsyncEnumerable<CommitData>>> ReadCommitsStreamAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken)
    {
        var result = await ReadCommitsAsync(repositoryPath, options, cancellationToken);

        return result.IsFailure
            ? Result<IAsyncEnumerable<CommitData>>.Failure(result.Error!)
            : Result<IAsyncEnumerable<CommitData>>.Success(StreamList(result.Value, cancellationToken));
    }

    /// <summary>
    /// Reads and maps commits into domain models.
    /// </summary>
    private async Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken)
    {
        return await Task.Run(async () =>
        {
            if (!Repository.IsValid(repositoryPath))
                return Result<IReadOnlyList<CommitData>>.Failure("Invalid Git repository");

            try
            {
                using var repo = new Repository(repositoryPath);

                var commitToBranches = options.IncludeBranches && RequiresBranchMap(repo)
                    ? BuildCommitToBranchMap(repo)
                    : null;

                var filter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Time | CommitSortStrategies.Topological
                };

                var commits = repo.Commits
                    .QueryBy(filter)
                    .Where(commit => IsCommitInRange(commit, options))
                    .Take(options.MaxCommits > 0 ? options.MaxCommits : int.MaxValue)
                    .ToList();

                if (!options.IncludeFileChanges)
                {
                    var commitResults = new List<CommitData>(commits.Count);
                    foreach (var commit in commits)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var result = MapCommit(
                            repo,
                            commit,
                            includeFileChanges: false,
                            options.IncludeBranches,
                            commitToBranches);

                        if (result.IsSuccess)
                            commitResults.Add(result.Value);
                    }

                    var orderedCommits = commitResults
                        .OrderBy(commit => commit.Timestamp.UnixSeconds)
                        .ToList()
                        .AsReadOnly();

                    logger.LogInformation("Read {Count} commits", orderedCommits.Count);

                    return Result<IReadOnlyList<CommitData>>.Success(orderedCommits);
                }

                var concurrentResults = new ConcurrentBag<CommitData>();
                var semaphore = new SemaphoreSlim(4);

                var tasks = commits
                    .Select(async commit =>
                    {
                        await semaphore.WaitAsync(cancellationToken);

                        try
                        {
                            if (cancellationToken.IsCancellationRequested)
                                return;

                            var result = MapCommit(
                                repo,
                                commit,
                                includeFileChanges: true,
                                options.IncludeBranches,
                                commitToBranches);

                            if (result.IsSuccess)
                                concurrentResults.Add(result.Value);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    })
                    .ToArray();

                await Task.WhenAll(tasks);

                var orderedConcurrentCommits = concurrentResults
                    .OrderBy(commit => commit.Timestamp.UnixSeconds)
                    .ToList()
                    .AsReadOnly();

                logger.LogInformation("Read {Count} commits", orderedConcurrentCommits.Count);

                return Result<IReadOnlyList<CommitData>>.Success(orderedConcurrentCommits);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read repository");

                return Result<IReadOnlyList<CommitData>>.Failure("Failed to read repository", ex);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Streams commits from an in-memory collection.
    /// </summary>
    private static async IAsyncEnumerable<CommitData> StreamList(
        IEnumerable<CommitData> commits,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var commit in commits)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return commit;
        }
    }

    /// <summary>
    /// Checks whether a commit matches date filters.
    /// </summary>
    private static bool IsCommitInRange(
        Commit commit,
        GitReaderOptions options)
    {
        return (!options.StartDate.HasValue || commit.Author.When >= options.StartDate.Value)
               && (!options.EndDate.HasValue || commit.Author.When <= options.EndDate.Value);
    }

    /// <summary>
    /// Builds a commit-to-branch lookup table.
    /// </summary>
    private Dictionary<string, List<string>> BuildCommitToBranchMap(
        Repository repo)
    {
        var map = new Dictionary<string, List<string>>();

        const int maxWalk = 1000;

        foreach (var branch in repo.Branches.Where(branch => !branch.IsRemote && branch.Tip != null))
        {
            var branchName = branch.FriendlyName;

            AddToMap(map, branch.Tip.Sha, branchName);

            foreach (var (commit, index) in repo.Commits
                         .QueryBy(new CommitFilter
                         {
                             IncludeReachableFrom = branch.Tip,
                             SortBy = CommitSortStrategies.Topological
                         })
                         .Select((commit, index) => (commit, index)))
            {
                AddToMap(map, commit.Sha, branchName);

                if (index + 1 >= maxWalk)
                    break;
            }
        }

        logger.LogDebug("Built branch map with {Count} commits", map.Count);

        return map;
    }

    /// <summary>
    /// Determines whether precise branch attribution requires a precomputed branch map.
    /// </summary>
    private static bool RequiresBranchMap(Repository repo)
        => repo.Branches.Count(branch => !branch.IsRemote && branch.Tip != null) > 1;

    /// <summary>
    /// Adds a branch entry to the lookup table.
    /// </summary>
    private static void AddToMap(
        Dictionary<string, List<string>> map,
        string sha,
        string branchName)
        => (map.TryGetValue(sha, out var list) ? list : map[sha] = []).Add(branchName);

    /// <summary>
    /// Converts a LibGit2Sharp commit into a domain model.
    /// </summary>
    private Result<CommitData> MapCommit(
        Repository repo,
        Commit commit,
        bool includeFileChanges,
        bool includeBranches,
        Dictionary<string, List<string>>? commitToBranches)
    {
        try
        {
            var basicsResult = MapCommitBasics(commit);
            if (basicsResult.IsFailure)
                return Result<CommitData>.Failure(basicsResult.Error!);

            var (sha, author, ts, parentShas) = basicsResult.Value;
            var fileChanges = includeFileChanges ? GetFileChanges(repo, commit) : [];
            var branches = includeBranches
                ? GetBranches(commit, repo, commitToBranches)
                : [];

            if (!branches.Any())
                logger.LogTrace("No branches found for commit {Sha}", commit.Sha[..8]);

            var commitData = new CommitData(
                Sha: sha,
                Author: author,
                Timestamp: ts,
                Message: commit.MessageShort,
                ParentShas: parentShas,
                FileChanges: fileChanges,
                Branches: branches,
                IsMerge: commit.Parents.Count() > 1);

            return Result<CommitData>.Success(commitData);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to map commit {Sha}", commit.Sha[..8]);

            return Result<CommitData>.Failure($"Failed to map commit {commit.Sha[..8]}", ex);
        }
    }

    /// <summary>
    /// Extracts validated commit metadata.
    /// </summary>
    private static Result<(
        CommitSha sha,
        ActorName author,
        Timestamp ts,
        List<CommitSha> parents)> MapCommitBasics(
        Commit commit)
    {
        var shaResult = CommitSha.Create(commit.Sha);
        if (shaResult.IsFailure)
            return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Failure(shaResult.Error!);

        var authorResult = ActorName.Create(commit.Author.Name);
        if (authorResult.IsFailure)
            return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Failure(authorResult.Error!);

        var timestampResult = Timestamp.Create(commit.Author.When.ToUnixTimeSeconds());
        if (timestampResult.IsFailure)
            return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Failure(timestampResult.Error!);

        var parentShas = commit.Parents
            .Select(parent => CommitSha.Create(parent.Sha))
            .Where(result => result.IsSuccess)
            .Select(result => result.Value)
            .ToList();

        return Result<(CommitSha, ActorName, Timestamp, List<CommitSha>)>.Success(
            (shaResult.Value, authorResult.Value, timestampResult.Value, parentShas));
    }

    /// <summary>
    /// Resolves branch names associated with a commit.
    /// </summary>
    private List<BranchName> GetBranches(
        Commit commit,
        Repository repo,
        Dictionary<string, List<string>>? commitToBranches)
    {
        if (commitToBranches != null
            && commitToBranches.TryGetValue(commit.Sha, out var branchNames))
        {
            return branchNames
                .Select(BranchName.Create)
                .Where(result => result.IsSuccess)
                .Select(result => result.Value)
                .ToList();
        }

        try
        {
            var head = repo.Head;

            if (head != null && !head.IsRemote)
            {
                var branchResult = BranchName.Create(head.FriendlyName);
                if (branchResult.IsSuccess)
                    return [branchResult.Value];
            }
        }
        catch
        {
            // Branch metadata is best-effort.
        }

        return [];
    }

    /// <summary>
    /// Extracts file changes for a commit diff.
    /// </summary>
    private IReadOnlyList<FileChange> GetFileChanges(
        Repository repo,
        Commit commit)
    {
        try
        {
            var parent = commit.Parents.FirstOrDefault();
            if (parent == null)
                return [];

            var changes = repo.Diff.Compare<TreeChanges>(parent.Tree, commit.Tree);
            var fileChanges = new List<FileChange>(changes.Count);

            foreach (var change in changes)
            {
                var pathResult = FilePath.Create(change.Path);
                if (pathResult.IsFailure)
                    continue;

                FilePath? oldPath = null;

                if (change is { Status: ChangeKind.Renamed, OldPath: not null })
                {
                    var oldPathResult = FilePath.Create(change.OldPath);
                    if (oldPathResult.IsSuccess)
                        oldPath = oldPathResult.Value;
                }

                fileChanges.Add(new FileChange(
                    Path: pathResult.Value,
                    Kind: MapChangeKind(change.Status),
                    OldPath: oldPath));
            }

            return fileChanges.AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get file changes for {Sha}", commit.Sha[..8]);

            return [];
        }
    }

    /// <summary>
    /// Converts LibGit2Sharp change types into domain change types.
    /// </summary>
    private static FileChangeKind MapChangeKind(
        ChangeKind kind) =>
        kind switch
        {
            ChangeKind.Added => FileChangeKind.Added,
            ChangeKind.Modified => FileChangeKind.Modified,
            ChangeKind.Deleted => FileChangeKind.Deleted,
            ChangeKind.Renamed => FileChangeKind.Renamed,
            _ => FileChangeKind.Modified
        };
}
